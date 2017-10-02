namespace TestFramework.Lib

open System
open System.Collections
open System.Security.Cryptography
open System.Security.Cryptography.X509Certificates
open System.Security.Cryptography.Xml
open System.Text
open System.Xml
open HttpFs.Client
open Newtonsoft.Json
open StatusCodes
open Mono.Security.X509
open Mono.Security.X509.Extensions
open Helpers

[<AutoOpen>]
module Licensing = 
    type public ActivateResult = 
        { ClientId : string
          ClientSecret : string }
    
    type ActivationParameters = 
        { ServiceId : string
          HostGuid : Guid
          License : string
          ProductName : string
          ProductVersion : string
          TabModuleId : int option }
    
    let defaultActivationParameters = 
        { ServiceId = "todolist"
          HostGuid = Guid.Empty
          License = "CLOUD-SOC-INTERNAL-2014-1"
          ProductName = "DNNCORP.F#.Automation"
          ProductVersion = "1.0"
          TabModuleId = None }
    
    type RegisterInstanceInput = 
        { InstanceId : Guid
          InstanceData : string }
    
    type ActivateSignedInput = 
        { ActivationInfo : string }
    
    type ErrorMessage = 
        { Message : string }
    
    let private toXml (activationParams : ActivationParameters) = 
        let output = StringBuilder()
        let writer = XmlWriter.Create(output)
        writer.WriteStartElement("ValidatedActivationInput")
        do writer.WriteAttributeString("version", "1.0")
           writer.WriteStartElement("Content")
           do writer.WriteElementString("ServiceId", activationParams.ServiceId)
              writer.WriteElementString("HostGUID", activationParams.HostGuid.ToString())
              writer.WriteElementString("License", activationParams.License)
              writer.WriteElementString("ProductName", activationParams.ProductName)
              writer.WriteElementString("ProductVersion", activationParams.ProductVersion)
              writer.WriteElementString("TabModuleId", 
                                        (match activationParams.TabModuleId with
                                         | Some(x) -> x
                                         | _ -> -1).ToString())
           writer.WriteEndElement()
        writer.WriteEndElement()
        writer.Close()
        output.ToString()
    
    let private createCertificate activationParams = 
        let subject = "Microservices"
        let sn = Guid.NewGuid().ToByteArray()
        if sn.[0] >= 128uy then sn.[0] <- sn.[0] - 128uy // serial number MUST be positive
        use issuerKey = new RSACryptoServiceProvider(2048)
        let subjectKey = issuerKey
        let bce = BasicConstraintsExtension()
        bce.PathLenConstraint <- BasicConstraintsExtension.NoPathLengthConstraint
        bce.CertificateAuthority <- true
        let cb = X509CertificateBuilder(3uy)
        cb.Hash <- "sha1"
        cb.SerialNumber <- sn
        cb.IssuerName <- sprintf "C=CA, ST=BC, L=langly, O=DNN Corp., OU=Engineering, CN=DNN-%s" subject
        cb.SubjectName <- cb.IssuerName
        cb.SubjectPublicKey <- subjectKey
        cb.NotBefore <- DateTime.UtcNow.AddSeconds(-1.0)
        cb.NotAfter <- DateTime.UtcNow.AddMonths(1)
        cb.Extensions.Add bce |> ignore
        let rawcert = cb.Sign issuerKey
        let password = activationParams.HostGuid.ToString()
        let pkcs12 = PKCS12()
        pkcs12.Password <- password
        let list = ArrayList()
        list.Add [| 1uy; 0uy; 0uy; 0uy |] |> ignore
        let attributes = Hashtable()
        attributes.Add(PKCS9.localKeyId, list)
        pkcs12.AddCertificate(X509Certificate(rawcert), attributes)
        pkcs12.AddPkcs8ShroudedKeyBag(subjectKey, attributes)
        new X509Certificate2(pkcs12.GetBytes(), password, (X509KeyStorageFlags.MachineKeySet + X509KeyStorageFlags.Exportable))
    
    let private toSignedXml (certificate : X509Certificate2) activationParams = 
        let xmlDoc = XmlDocument()
        xmlDoc.PreserveWhitespace <- true
        activationParams
        |> toXml
        |> xmlDoc.LoadXml
        let signedXml = SignedXml xmlDoc
        signedXml.SigningKey <- certificate.PrivateKey
        let reference = Reference()
        reference.Uri <- String.Empty
        XmlDsigEnvelopedSignatureTransform() |> reference.AddTransform
        signedXml.AddReference(reference)
        signedXml.ComputeSignature()
        let xmlDigitalSignature = signedXml.GetXml()
        xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true)) |> ignore
        xmlDoc.OuterXml
    
    let registerInstance serviceUrl activationParams = 
        let certificate = activationParams |> createCertificate
        
        let regInfo = 
            { InstanceId = activationParams.HostGuid
              InstanceData = Convert.ToBase64String(certificate.Export(X509ContentType.Cert)) }
        
        // the following are used for BasicAuthentication of the service to the licensing server
        //let private testClientName = "automation-test-client"
        let automationClientId = "dnn-automation-test-client"
        let automationClientSecret = "Test"
        let uri = uriFor serviceUrl "/api/License/RegisterInstance"
        let request = postTo uri |> withJsonBody regInfo
        //|> Request.basicAuthentication automationClientId automationClientSecret
        let response = request |> getResponse
        (//printfn "\r\nREQUEST =>\r\n%A\r\n" request
         //printfn "\r\nRESPONSE =>\r\n%A\r\n" response
         certificate, response)
    
    let activateInstance serviceUrl (certificate : X509Certificate2) activationParams = 
        let signedXml : string = activationParams |> toSignedXml certificate
        let signedParameters = { ActivationInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes signedXml) }
        let uri = uriFor serviceUrl "/api/License/Activate"
        let request = postTo uri |> withJsonBody signedParameters
        let response = request |> getResponse
        let content = response |> getBody
        if response.statusCode = ok then 
            printfn "\r\nCONTENT =>\r\n%A\r\n" content
            (JsonConvert.DeserializeObject<ActivateResult>(content), null)
        else 
            printfn "\r\nREQUEST =>\r\n%A\r\n" request
            printfn "\r\nRESPONSE =>\r\n%A\r\n" response
            let error = 
                if response.statusCode = badRequest then JsonConvert.DeserializeObject<ErrorMessage>(content).Message
                else sprintf "%A: %A" response.statusCode content
            
            let activation = 
                { ClientId = null
                  ClientSecret = null }
            
            (activation, error)
    
    let getClientIdAndSecret serviceUrl activationParams = 
        let cert, response = activationParams |> registerInstance serviceUrl
        //System.Threading.Thread.Sleep(3000)
        if response.statusCode = ok then 
            let activateResult, error = activationParams |> activateInstance serviceUrl cert
            (//System.Threading.Thread.Sleep(5000)
             activateResult.ClientId, activateResult.ClientSecret)
        else (null, null)
