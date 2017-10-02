module ExcelSheet

open Microsoft.Office.Interop

let mutable value1 = ""

let functcalulate value =
    printfn " value of element in excel %A" value
    //let v = int(value)
    //printfn "%d" v

let private excelworksheettest () =
    // Start Excel, Open a exiting file for input and create a new file for output
    let xlApp = new Excel.ApplicationClass()
    let xlWorkBookInput = xlApp.Workbooks.Open(@"C:\Users\amritpal.manak\Desktop\Input.xlsx")
    let xlWorkBookOutput = xlApp.Workbooks.Add()
    xlApp.Visible <- true

    // Open input's 'Sheet1' and create a new worksheet in output.xlsx

    let xlWorkSheetInput = xlWorkBookInput.Worksheets.["Sheet1"] :?> Excel.Worksheet
    let xlWorkSheetOutput = xlWorkBookOutput.Worksheets.[1] :?> Excel.Worksheet
    xlWorkSheetOutput.Name <- "OutputSheet1"

    // EXAMPLE 1: Reading\Writing a cell value using cell index
    let value1 = xlWorkSheetInput.Cells.[10,5]
    xlWorkSheetOutput.Cells.[10,5] <- value1
 
    // EXAMPLE 1.1: Reading\Writing a cell value using range
    let value2 = xlWorkSheetInput.Cells.Range("A1","A8").Value2
    xlWorkSheetOutput.Cells.Range("A1","A8").Value2 <- value2

    // EXAMPLE 2: Reading\Writing a row
    let row = xlWorkSheetInput.Cells.Rows.[1] :?> Excel.Range
    (xlWorkSheetOutput.Cells.Rows.[1] :?> Excel.Range).Value2 <- row.Value2

    // EXAMPLE 3: Reading\Writing a column
    let column1 = xlWorkSheetInput.Cells.Range("B:B")
    xlWorkSheetOutput.Cells.Range("B:B").Value2 <- column1.Value2

    // EXAMPLE 4: Reading\Writing a Range
    let inputRange = xlWorkSheetInput.Cells.Range("A1","E10")
    for i in 1 .. inputRange.Cells.Rows.Count do
        for j in 1 .. inputRange.Cells.Columns.Count  do
            xlWorkSheetOutput.Cells.[i,j] <- inputRange.[i,j]

    // EXAMPLE 5: Writing an Jagged arrays
    let data =  [|  [|0 .. 1 .. 2|];
                    [|0 .. 1 .. 4|];
                    [|0 .. 1 .. 6|] |]
 
    for i in 1 .. data.Length do
        for j in 1 .. data.[i-1].Length do
            xlWorkSheetOutput.Cells.[j, i] <- data.[i-1].[j-1]

//     EXAMPLE 6: Reading\Writing a Range
//    let mutable value1: string = ""
//    let mutable value1 = xlWorkSheetInput.Cells.[10,5]
    let inputRange = xlWorkSheetInput.Cells.Range("A1","A10")
    for i in 1 .. inputRange.Cells.Rows.Count do
        for j in 1 .. inputRange.Cells.Columns.Count  do
           let mutable value1 = xlWorkSheetInput.Cells.[i,j]
           functcalulate value1

//let inputform field = 
//    logOff()
//    loginAsContentManager() |> ignore 
//    openPBForms()
//    let formTitle = createFormAndVerify "Single-Line Text"
//    let hiddenFieldValue = field
//    addHiddenField formTitle "ClientID" hiddenFieldValue 
//    pageUrl <- deployFormOnNewPage formTitle "singleStep"
//    let fieldValue = "1234"
//    addValueToSingleLineForm fieldValue
//    checkResponseTable formTitle hiddenFieldValue fieldValue


let positive _ = 
    excelworksheettest()


let all _ =
    positive()           