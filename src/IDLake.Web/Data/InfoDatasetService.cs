using IDLake.Models;
using Microsoft.EntityFrameworkCore;
using IDLake.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDLake.Web.Data
{
    public class InfoDatasetService : ICrud<InfoDataset>
    {
        IDLakeDB db;

        public InfoDatasetService()
        {
            if (db == null) db = new IDLakeDB();

        }
        public bool DeleteData(object Id)
        {
            var selData = (db.InfoDatasets.Where(x => x.Id == (long)Id).FirstOrDefault());
            db.InfoDatasets.Remove(selData);
            db.SaveChanges();
            return true;
        }

        public List<InfoDataset> FindByKeyword(string Keyword,string Kategori="")
        {
            bool IsAll = string.IsNullOrEmpty(Keyword) ? true : false;
            var data = from x in db.InfoDatasets
                       where x.Nama.Contains(Keyword) || x.Deskripsi.Contains(Keyword) || IsAll
                       select x;
            if (!string.IsNullOrEmpty(Kategori) && Kategori!="Semua")
            {
                data.Where(x => x.Category == Kategori);
            }
            return data.ToList();
        } 
        
        public List<InfoDataset> FindByKeyword(string Keyword)
        {
            bool IsAll = string.IsNullOrEmpty(Keyword) ? true : false;
            var data = from x in db.InfoDatasets
                       where x.Nama.Contains(Keyword) || x.Deskripsi.Contains(Keyword) || IsAll
                       select x;
           
            return data.ToList();
        }

        public List<InfoDataset> GetAllData()
        {
            return db.InfoDatasets.ToList();
        } 
        
        public List<string> GetPublicNames()
        {
            var datas = db.InfoDatasets.Where(x=>x.AccessType == AccessTypes.Public).OrderBy(x=>x.Nama);
            return datas.Select(x => x.Nama).Distinct().ToList();
        } 
        public List<string> GetCategory()
        {
            var datas = db.InfoDatasets.Where(x=>x.AccessType == AccessTypes.Public).OrderBy(x=>x.Category);
            return datas.Select(x => x.Category).Distinct().ToList();
        }

        public InfoDataset GetDataById(object Id)
        {
            return db.InfoDatasets.Where(x => x.Id == (long)Id).FirstOrDefault();
        }
           public InfoDataset GetDataByUid(string Uid)
        {
            return db.InfoDatasets.Where(x => x.UniqueId == Uid).FirstOrDefault();
        }


        public bool InsertData(InfoDataset data)
        {
            try
            {
                db.InfoDatasets.Add(data);
                db.SaveChanges();
                return true;
            }
            catch
            {

            }
            return false;

        }



        public bool UpdateData(InfoDataset data)
        {
            try
            {
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();

                /*
                if (sel != null)
                {
                    sel.Nama = data.Nama;
                    sel.Keterangan = data.Keterangan;
                    sel.Tanggal = data.Tanggal;
                    sel.DocumentUrl = data.DocumentUrl;
                    sel.StreamUrl = data.StreamUrl;
                    return true;

                }*/
                return true;
            }
            catch
            {

            }
            return false;
        }

        public long GetLastId()
        {
            return db.InfoDatasets.Max(x => x.Id);
        }
    }

}