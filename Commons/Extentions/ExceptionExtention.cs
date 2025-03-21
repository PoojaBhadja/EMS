using Commons.Constants;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Extentions
{
    public static class ExceptionExtention
    {
        public static void CheckAndThrowNullException<T>(this T obj)
        {
            if (obj is null)
            {
                throw new EntryPointNotFoundException(ErrorMessages.RequestParameterIsNotProper);
            }
        }

        public static void CheckAndThrowNullExceptionWithCustomeMessage<T>(this T obj, string message)
        {
            if (obj is null)
            {
                throw new EntryPointNotFoundException(message);
            }
        }

        public static void ThrowDublicateException<T>(this T obj)
        {
            throw new DuplicateNameException(ErrorMessages.DublicateDataFound);
        }

        public static void ThrowNotFoundException<T>(this T obj)
        {
            throw new DuplicateNameException(ErrorMessages.DataNotFound);
        }
    }
}

