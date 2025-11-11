using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AventasApi.Models.Traslado.DTOs
{
    public class MemoryPostedFile : HttpPostedFileBase
    {
        private readonly byte[] _content;
        private readonly string _fileName;
        private readonly string _contentType;

        public MemoryPostedFile(byte[] content, string fileName, string contentType)
        {
            _content = content;
            _fileName = fileName;
            _contentType = contentType;
        }

        public override int ContentLength => _content.Length;
        public override string FileName => _fileName;
        public override Stream InputStream => new MemoryStream(_content);
        public override string ContentType => _contentType;
    }
}