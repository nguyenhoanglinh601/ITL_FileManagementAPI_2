
using FileManagementAPI.Common;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementAPI.Infrastructure
{
    public static class HandleMessage
    {
        public static String Get( HandleState hs)
        {
            switch (hs.Code)
            {
                
                case 201:
                    return LanguageSub.MSG_OBJECT_NOT_EXISTS;
                case 203:
                    return LanguageSub.MSG_OBJECT_DUPLICATED;
                case 204:
                    return LanguageSub.MSG_OBJECT_RELATION_NOT_VALID;

            }
            switch (hs.Exception.Message)
            {
                case "":
                    return "";
            }
            return LanguageSub.MSG_DATA_NOT_FOUND;
        }
        public static String Add( HandleState hs)
        {
            switch (hs.Code)
            {
                case 200:
                    return LanguageSub.MSG_INSERT_SUCCESS;
                case 201:
                    return LanguageSub.MSG_OBJECT_NOT_EXISTS;
                case 203:
                    return LanguageSub.MSG_OBJECT_DUPLICATED;
                case 204:
                    return LanguageSub.MSG_OBJECT_RELATION_NOT_VALID;
                
            }
            switch (hs.Exception.Message)
            {
                case "":
                    return "";
            }
           return LanguageSub.MSG_DATA_NOT_FOUND;
        }
        public static String Update( HandleState hs)
        {
            switch (hs.Code)
            {
                case 200:
                    return LanguageSub.MSG_UPDATE_SUCCESS;
                case 201:
                    return LanguageSub.MSG_OBJECT_NOT_EXISTS;
                case 203:
                    return LanguageSub.MSG_OBJECT_DUPLICATED;
                case 204:
                    return LanguageSub.MSG_OBJECT_RELATION_NOT_VALID;

            }
            switch (hs.Exception.Message)
            {
                case "":
                    return "";
            }
            return LanguageSub.MSG_DATA_NOT_FOUND;
        }
        public static String Delete( HandleState hs)
        {
            switch (hs.Code)
            {
                case 200:
                    return LanguageSub.MSG_DELETE_SUCCESS;
                case 202:
                    return LanguageSub.MSG_DELETE_FAIL_INCLUDED_CHILD;
                case 201:
                    return LanguageSub.MSG_OBJECT_NOT_EXISTS;
                case 203:
                    return LanguageSub.MSG_OBJECT_DUPLICATED;
                case 204:
                    return LanguageSub.MSG_OBJECT_RELATION_NOT_VALID;

            }
            switch (hs.Exception.Message)
            {
                case "":
                    return "";
            }
            return LanguageSub.MSG_DATA_NOT_FOUND;
        }
    }
}
