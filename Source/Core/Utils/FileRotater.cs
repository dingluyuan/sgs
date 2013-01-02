﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sanguosha.Core.Utils
{
    /// <summary>
    /// Maintains a file sequence fileName{Date}{Sequence number} and removes the most out dated file when
    /// number of files exceeds a certain threshold.
    /// </summary>
    /// <remarks>Not thread-safe.</remarks>
    public class FileRotator
    {       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathName">Path to the directory under which the new file is to be created.</param>
        /// <param name="fileName">Common prefix of the file name.</param>
        /// <param name="extension">Extension name of the file. Must starts with "."</param>
        /// <param name="maxAllowance">Maximum number of files allowed before rotation starts. Must be greater or equal to 0.</param>
        /// <returns></returns>
        public static FileStream CreateFile(string pathName, string fileName, string extension, int maxAllowance)
        {
            if (!Directory.Exists(pathName))
            {
                Directory.CreateDirectory(pathName);
            }
            var filePaths = Directory.EnumerateFiles(pathName);

            var suspects = from filePath in filePaths
                           where Path.GetFileName(filePath).StartsWith(fileName) && pathName.EndsWith(extension)
                           orderby File.GetCreationTime(filePath)
                           select filePath;
             
            int total = suspects.Count();
            if (total > maxAllowance)
            {
                foreach (var filePath in suspects.Take(total - maxAllowance))
                {
                    File.Delete(filePath);
                }
            }            
            DateTime dt = DateTime.Now;
            string newFile = string.Format("{0}/{1}{2:yyyymmdd}{3}{4}", pathName, fileName, dt, 
                                           (int)dt.TimeOfDay.TotalMilliseconds, extension);
            FileStream fs = new FileStream(newFile, FileMode.Create);

            return fs;
        }

    }
}
