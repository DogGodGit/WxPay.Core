﻿using System;
using System.IO;

namespace WxPayAPI
{
    public class Log
    {
        /// <summary>
        /// 向日志写入调试信息
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="content">写入内容</param>
        public static void Debug(string className, string content)
        {
            if (WxPayConfig.GetConfig().GetLogLevel() >= 3)
            {
                WriteLog("DEBUG", className, content);
            }
        }

        /// <summary>
        /// 向日志写入运行时信息
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="content">写入内容</param>
        public static void Info(string className, string content)
        {
            if (WxPayConfig.GetConfig().GetLogLevel() >= 2)
            {
                WriteLog("INFO", className, content);
            }
        }

        /// <summary>
        /// 向日志写入出错信息
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="content">写入内容</param>
        public static void Error(string className, string content)
        {
            if (WxPayConfig.GetConfig().GetLogLevel() >= 1)
            {
                WriteLog("ERROR", className, content);
            }
        }

        /// <summary>
        /// 实际的写日志操作
        /// </summary>
        /// <param name="type">日志记录类型</param>
        /// <param name="className">类名</param>
        /// <param name="content">写入内容</param>
        protected static void WriteLog(string type, string className, string content)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");//获取当前系统时间

            //日志内容
            string write_content = time + " " + type + " " + className + ": " + content;

            //需要用户自定义日志实现形式
            Console.WriteLine(write_content);

            var dir = AppDomain.CurrentDomain.BaseDirectory + @"\log\";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var fileName = dir + type + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

            //没有则创建这个文件
            if (!File.Exists(fileName))
            {
                File.Create(fileName);
            }

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine("-----------");
                sw.WriteLine(write_content);
            }
        }
    }
}