using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Meteo.IO
{
    public class MatrixFile
    {
        protected DenseMatrix _matrix = null;
        protected string _fileName = string.Empty;

        public DenseMatrix Matrix
        {
            get { return _matrix.Clone() as DenseMatrix; }
            set { _matrix = value.Clone() as DenseMatrix; }
        }

        static MatrixFile()
        {
        }

        public void Save(string fmt = null)
        {
            InternalSave(fmt);
        }

        public MatrixFile(string fileName)
        {
            _fileName = fileName;
            InternalLoad();
        }

        protected virtual void InternalLoad()
        {
        }

        protected virtual void InternalSave(string fmt = null)
        {
        }

    }
}
