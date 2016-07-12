﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace rawtrack.TrackData
{
    public class TrackInfo
    {
        public string outFileName = "";
        public int inputFrameLength = 937;
        public int outputFrameLength = 886;
        public int frameShiftLength = 47;
    }

    // 数据抽取框架，遍历输入文件列表，对每个文件抽取指定长度帧，通过TrackDataCore
    // 将抽取出的数据写入输出文件
    public class TrackDataFrame
    {
        private FileStream outFileStream = null;
        private BinaryWriter outFileWriter = null;
        public TrackInfo OutInfo
        {
            get;
            set;
        }

        internal bool TrackFile(BinaryReader brInFile)
        {
            if (this.OutInfo.outFileName == "")
            {
                return false;
            }
            if (outFileStream == null)
            {
                outFileStream = new FileStream(this.OutInfo.outFileName, FileMode.Create);
                this.outFileWriter = new BinaryWriter(outFileStream);
            }

            if (brInFile.BaseStream.Length == 0 || brInFile.BaseStream.Length % OutInfo.inputFrameLength != 0)
            {
                Close();
                return false;
            }

                TrackBlock(brInFile);
            return true;
        }

        private void TrackBlock(BinaryReader br)
        {
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                long blockSize = this.OutInfo.inputFrameLength * 100;
                if (blockSize > br.BaseStream.Length - br.BaseStream.Position)
                {
                    blockSize = br.BaseStream.Length - br.BaseStream.Position;
                }

                long blockNum = blockSize / OutInfo.inputFrameLength;

                byte[] buffer = br.ReadBytes((int)blockSize);
                byte[] bufferOut = new byte[blockNum * OutInfo.outputFrameLength];

                for (int i = 0; i < blockNum; i++)
                {
                    Buffer.BlockCopy(buffer, OutInfo.inputFrameLength * i + OutInfo.frameShiftLength,
                        bufferOut, OutInfo.outputFrameLength * i, OutInfo.outputFrameLength);
                }

                outFileWriter.Write(bufferOut);

            }
        }

        public void Close()
        {
            if (outFileStream != null)
            {
                outFileWriter.Close();
                outFileStream.Close();
            }
            outFileStream = null;
            outFileWriter = null;
        }
    } 
}
