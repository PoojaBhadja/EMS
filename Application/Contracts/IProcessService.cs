using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Http;
using Models.Entities;
using Models.ViewModels;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IProcessService
    {
        List<ExpenceIncome> UploadExcel(IFormFile file);
    }
}
