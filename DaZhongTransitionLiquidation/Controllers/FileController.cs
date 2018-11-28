using DaZhongTransitionLiquidation.Infrastructure.Dao;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaZhongTransitionLiquidation.Controllers
{
    public class FileController: BaseController
    {
        private string tempImgPath = FileSugar.GetMapPath("~/_theme/temp/img");
        private string tempFilePath = FileSugar.GetMapPath("~/_theme/temp/file");
        public FileController(DbService dbService, DbBusinessDataService dbBusinessDataService) : base(dbService, dbBusinessDataService)
        {

        }
        public JsonResult UploadImage(int allowSize = 2)
        {
            UploadImage ui = new UploadImage();
            ui.SetAllowSize = allowSize;
            var postFile = System.Web.HttpContext.Current.Request.Files[0];
            var responseMessage = ui.FileSaveAs(postFile, tempImgPath);

            return Json(responseMessage, "text/plain;charset=utf-8", JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadFile(int allowSize = 20, string fileType = ".docx,.doc,.xlsx,.xls,.txt,.ppt,.jpg,.png,.gif,.pdf")
        {
            UploadFile uf = new UploadFile();
            uf.SetMaxSizeM(allowSize);
            uf.SetFileDirectory(tempFilePath);
            uf.SetIsRenameSameFile(true);
            uf.SetFileType(fileType);
            HttpPostedFileBase postFile = new HttpPostedFileWrapper(System.Web.HttpContext.Current.Request.Files[0]) as HttpPostedFileBase;
            var responseMessage = uf.Save(postFile);

            return Json(responseMessage, "text/plain;charset=utf-8", JsonRequestBehavior.AllowGet);
        }
    }
}