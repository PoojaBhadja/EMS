using Application.Contracts;
using AutoMapper;
using Commons.Enums;
using Commons.Helpers;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using Models.Entities;
using Models.ViewModels;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ExcelService
    {

        public static async Task test()
        {
            string filePath = @"C:\Users\Pooja Bhadja\Desktop\database backup\transaction.csv";
            string connectionString = "Server=tcp:expence-managment-system.database.windows.net,1433;Initial Catalog=ExpenceManagmentSystem;Persist Security Info=False;User ID=ExpenceManagmentSystemAdmin;Password=rfC0aFZgoX$3azJE;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            string tableName = "Transaction"; // SQL Table Name

            DataTable dt = ReadCsvToDataTable_User(filePath);
            //await BulkInsertToAzureSQL_User(dt, connectionString, tableName);
            await BulkInsertToAzureSQL_Transaction(dt, connectionString, tableName);
        }

        static async Task BulkInsertToAzureSQL_CardHolder(DataTable dataTable, string connectionString, string tableName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                    {
                        bulkCopy.DestinationTableName = tableName;

                        // Map CSV columns to SQL columns
                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.ColumnMappings.Add("CardHolderName", "CardHolderName");
                        bulkCopy.ColumnMappings.Add("DisplayName", "DisplayName");
                        bulkCopy.ColumnMappings.Add("Balance", "Balance");

                        bulkCopy.ColumnMappings.Add("CreatedBy", "CreatedBy");
                        bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate");
                        bulkCopy.ColumnMappings.Add("UpdatedBy", "UpdatedBy");
                        bulkCopy.ColumnMappings.Add("UpdatedDate", "UpdatedDate");
                        bulkCopy.ColumnMappings.Add("IsActive", "IsActive");

                        await bulkCopy.WriteToServerAsync(dataTable);
                    }
                }
            }
            catch (Exception e) { }


        }
        static async Task BulkInsertToAzureSQL_Category(DataTable dataTable, string connectionString, string tableName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                    {
                        bulkCopy.DestinationTableName = tableName;

                        // Map CSV columns to SQL columns
                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.ColumnMappings.Add("Name", "Name");
                        bulkCopy.ColumnMappings.Add("CategoryType", "CategoryType");

                        bulkCopy.ColumnMappings.Add("CreatedBy", "CreatedBy");
                        //bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate");
                        //bulkCopy.ColumnMappings.Add("UpdatedBy", "UpdatedBy");
                        //bulkCopy.ColumnMappings.Add("UpdatedDate", "UpdatedDate");
                        bulkCopy.ColumnMappings.Add("IsActive", "IsActive");

                        await bulkCopy.WriteToServerAsync(dataTable);
                    }
                }
            }
            catch (Exception e) { }


        }
        static async Task BulkInsertToAzureSQL_SubCategory(DataTable dataTable, string connectionString, string tableName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                    {
                        bulkCopy.DestinationTableName = tableName;

                        // Map CSV columns to SQL columns
                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.ColumnMappings.Add("Name", "Name");
                        bulkCopy.ColumnMappings.Add("CategoryId", "CategoryId");

                        bulkCopy.ColumnMappings.Add("CreatedBy", "CreatedBy");
                        bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate");
                        //bulkCopy.ColumnMappings.Add("UpdatedBy", "UpdatedBy");
                        //bulkCopy.ColumnMappings.Add("UpdatedDate", "UpdatedDate");
                        bulkCopy.ColumnMappings.Add("IsActive", "IsActive");

                        await bulkCopy.WriteToServerAsync(dataTable);
                    }
                }
            }
            catch (Exception e) { }


        }

        static async Task BulkInsertToAzureSQL_Transaction(DataTable dataTable, string connectionString, string tableName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                    {
                        bulkCopy.DestinationTableName = tableName;

                        // Map CSV columns to SQL columns
                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.ColumnMappings.Add("Description", "Description");
                        bulkCopy.ColumnMappings.Add("CategoryId", "CategoryId");
                        bulkCopy.ColumnMappings.Add("SubCategoryId", "SubCategoryId");
                        bulkCopy.ColumnMappings.Add("CardHolderId", "CardHolderId");
                        bulkCopy.ColumnMappings.Add("Amount", "Amount");
                        bulkCopy.ColumnMappings.Add("TransactionType", "TransactionType");
                        bulkCopy.ColumnMappings.Add("TransactionDate", "TransactionDate");

                        bulkCopy.ColumnMappings.Add("CreatedBy", "CreatedBy");
                        bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate");
                        //bulkCopy.ColumnMappings.Add("UpdatedBy", "UpdatedBy");
                        //bulkCopy.ColumnMappings.Add("UpdatedDate", "UpdatedDate");
                        bulkCopy.ColumnMappings.Add("IsActive", "IsActive");

                        await bulkCopy.WriteToServerAsync(dataTable);
                    }
                }
            }
            catch (Exception e) { }


        }

        static async Task BulkInsertToAzureSQL_User(DataTable dataTable, string connectionString, string tableName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                {
                    bulkCopy.DestinationTableName = tableName;

                    // Map CSV columns to SQL columns
                    bulkCopy.ColumnMappings.Add("Id", "Id");
                    bulkCopy.ColumnMappings.Add("Email", "Email");
                    bulkCopy.ColumnMappings.Add("UserName", "UserName");
                    bulkCopy.ColumnMappings.Add("Password", "Password");
                    bulkCopy.ColumnMappings.Add("IsActive", "IsActive");

                    await bulkCopy.WriteToServerAsync(dataTable);
                }
            }

        }

        static DataTable ReadCsvToDataTable_User(string filePath)
        {
            DataTable dt = new DataTable();
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string[] headers = sr.ReadLine().Split(',');

                    foreach (string header in headers)
                    {
                        string colName = header.Trim().ToLower();

                        if (colName == "id" || colName == "createdby" || colName == "categoryid" 
                            || colName == "subcategoryid" || colName == "cardholderid")
                            dt.Columns.Add(header.Trim(), typeof(Guid));
                        else if (colName == "isactive")
                            dt.Columns.Add(header.Trim(), typeof(bool));
                        else if (colName == "amount")
                            dt.Columns.Add(header.Trim(), typeof(decimal));
                        else if (colName == "transactiontype")
                            dt.Columns.Add(header.Trim(), typeof(int));
                        //else if (colName == "createddate" || colName == "updateddate")
                        //    dt.Columns.Add(header.Trim(), typeof(DateTime));
                        else
                            dt.Columns.Add(header.Trim(), typeof(string));
                    }

                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        DataRow dr = dt.NewRow();

                        for (int i = 0; i < headers.Length; i++)
                        {
                            string colName = headers[i].Trim().ToLower();

                            if (colName == "id" || colName == "createdby" || colName == "categoryid" 
                                || colName == "subcategoryid" || colName == "cardholderid")
                                dr[i] = rows[i].Trim().ToLower() == "null" ? string.Empty : Guid.Parse(rows[i].Trim());
                            else if (colName == "isactive")
                                dr[i] = Convert.ToBoolean(rows[i].Trim() == "1" ? "true" : "false");
                            else if (colName == "amount")
                                dr[i] = Convert.ToDecimal(rows[i].Trim());
                            else if (colName == "transactiontype")
                                dr[i] = Convert.ToInt32(rows[i].Trim());
                            //else if (colName == "createddate" || colName == "updateddate")
                            //    dr[i] = Convert.ToDateTime(rows[i].Trim());
                            else
                                dr[i] = rows[i].Trim();
                        }
                        dt.Rows.Add(dr);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return dt;
        }

        public static List<User> UploadCashholder(IFormFile file)
        {
            try
            {
                ExcelWorksheets excelWorksheets = null;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excelPackage = new ExcelPackage();

                using (var stream = file.OpenReadStream())
                {
                    excelPackage.Load(stream);
                }

                excelWorksheets = excelPackage.Workbook.Worksheets;
                ExcelWorksheet excelWorksheet = excelWorksheets[0];
                return ConvertExcelDataToObject_Users(excelWorksheet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        //public static MemoryStream Download(List<ExpenceIncomeVm> data)
        //{
        //    try
        //    {
        //        var stream = new MemoryStream();

        //        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        //        {
        //            var workbookPart = document.AddWorkbookPart();
        //            workbookPart.Workbook = new Workbook();

        //            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        //            worksheetPart.Worksheet = new Worksheet();

        //            var workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
        //            //GenerateWorkbookStylesPartContent(workbookStylesPart);

        //            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        //            var sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Debit/Credit Transaction" };
        //            sheets.Append(sheet);

        //            workbookPart.Workbook.Save();

        //            var sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

        //            foreach (var item in data.Select((value, i) => new { i, value }))
        //            {
        //                if (item.i == 0)
        //                {
        //                    var headerRow = new Row();
        //                    headerRow.Append(
        //                        new Cell()
        //                        {
        //                            CellValue = new CellValue(nameof(item.value.SpentDate)),
        //                            DataType = new EnumValue<CellValues>(CellValues.String)
        //                        },
        //                        new Cell()
        //                        {
        //                            CellValue = new CellValue(nameof(item.value.Description)),
        //                            DataType = new EnumValue<CellValues>(CellValues.String)
        //                        },
        //                        new Cell()
        //                        {
        //                            CellValue = new CellValue(nameof(item.value.Amount)),
        //                            DataType = new EnumValue<CellValues>(CellValues.String)
        //                        },
        //                        new Cell()
        //                        {
        //                            CellValue = new CellValue(nameof(item.value.CardHolderName)),
        //                            DataType = new EnumValue<CellValues>(CellValues.String)
        //                        },
        //                        new Cell()
        //                        {
        //                            CellValue = new CellValue(nameof(item.value.TransactionType)),
        //                            DataType = new EnumValue<CellValues>(CellValues.String)
        //                        });
        //                    sheetData.AppendChild(headerRow);
        //                }


        //                var row = new Row();
        //                var cell = new Cell();

        //                cell.CellValue = new CellValue() { Text = ((DateTime)item.value.SpentDate).ToOADate().ToString(CultureInfo.InvariantCulture) };
        //                cell.DataType = new EnumValue<CellValues>(CellValues.Date);

        //                cell.CellValue = new CellValue(item.value.Description ?? string.Empty);
        //                cell.DataType = new EnumValue<CellValues>(CellValues.String);

        //                cell.CellValue = new CellValue(Convert.ToString(item.value.Amount, CultureInfo.InvariantCulture));
        //                cell.DataType = new EnumValue<CellValues>(CellValues.Number);

        //                cell.CellValue = new CellValue(item.value.CardHolderName ?? string.Empty);
        //                cell.DataType = new EnumValue<CellValues>(CellValues.String);

        //                cell.CellValue = new CellValue(Convert.ToString(item.value.TransactionType, CultureInfo.InvariantCulture));
        //                cell.DataType = new EnumValue<CellValues>(CellValues.String);

        //                row.Append(cell);

        //                sheetData.AppendChild(row);
        //            }


        //            workbookPart.Workbook.Save();
        //        }

        //        if (stream?.Length > 0)
        //        {
        //            stream.Seek(0, SeekOrigin.Begin);
        //        }

        //        return stream;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        private static List<User> ConvertExcelDataToObject_Users(ExcelWorksheet excelWorksheet)
        {
            List<User> ExpenceIncomeList = new List<User>();
            int colCount = excelWorksheet.Dimension.End.Column;  //get Column Count
            var Dimension = excelWorksheet.Dimension;     //get row count
            for (int i = 2; i <= Dimension.End.Row; i++)
            {
                User expenceIncome = new User();
                expenceIncome.Id = Guid.Parse(excelWorksheet.Cells[i, 1].Value.ToString());
                expenceIncome.Email = Convert.ToString(excelWorksheet.Cells[i, 2].Value);
                expenceIncome.UserName = Convert.ToString(excelWorksheet.Cells[i, 3].Value);
                expenceIncome.Password = Convert.ToString(excelWorksheet.Cells[i, 4].Value);
                //expenceIncome.CreatedBy = Guid.Parse(excelWorksheet.Cells[i, 5].Value.ToString());
                //expenceIncome.CreatedDate = Convert.ToInt32(excelWorksheet.Cells[i, 8].Value);
                expenceIncome.IsActive = Convert.ToBoolean(excelWorksheet.Cells[i, 8].Value);
                ExpenceIncomeList.Add(expenceIncome);

            }

            return ExpenceIncomeList;
        }
    }
}
