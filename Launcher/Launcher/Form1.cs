using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using IWshRuntimeLibrary;

namespace Launcher
{
    
    public partial class Form1 : Form
    {
        List<FileInfo> allFoundFiles = new List<FileInfo>();
        List<Program> pro = new List<Program>();

        public Form1()
        {
            InitializeComponent();
            string dir = System.IO.Path.GetDirectoryName(Application.ExecutablePath); 
            string serializationFile = Path.Combine(dir, "program.bin");
            if (System.IO.File.Exists(serializationFile))
            {
                using (Stream stream = System.IO.File.Open(serializationFile, FileMode.Open))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    pro = (List<Program>)bformatter.Deserialize(stream);
                }
            }
        }

        [Serializable]
        public class Program
        {
            public FileInfo file;
            public string path;
            public int count = 0;
            public Program(FileInfo f)
            {
                file = f;
                path = f.FullName;
                count = 1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            DirectoryInfo dirInfo = new DirectoryInfo(textBox1.Text);
            allFoundFiles = Filef.GetFilesByExtensions(dirInfo, ".exe", ".lnk").ToList();
            for (int i = 0; i < allFoundFiles.Count; i++)
            {
                if (allFoundFiles[i].Extension == ".lnk")
                {
                    allFoundFiles[i] = new FileInfo(Filef.lnk(allFoundFiles[i]));

                    bool find = false;
                    int index = 0;
                    for (int j = 0; j < pro.Count; j++)
                    {
                        if (pro[j].path == allFoundFiles[i].FullName)
                        {
                            find = true;
                            index = j;
                        }
                    }
                    if (find) listBox1.Items.Add(allFoundFiles[i].Name + " Запусков программы: " + pro[index].count);
                    else listBox1.Items.Add(allFoundFiles[i].Name);
                }
                else
                {
                    listBox1.Items.Add(allFoundFiles[i].Name);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                FileInfo file = allFoundFiles[listBox1.SelectedIndex];
                Process iStartProcess = new Process();
                iStartProcess.StartInfo.FileName = file.ToString();
                iStartProcess.Start();
                bool find = false;
                for (int i = 0; i < pro.Count; i++)
                {
                    if (pro[i].path == file.FullName)
                    {
                        pro[i].count++;
                        find = true;
                    }
                }
                if (!find) pro.Add(new Program(allFoundFiles[listBox1.SelectedIndex]));

                listBox1.Items.Clear();
                DirectoryInfo dirInfo = new DirectoryInfo(textBox1.Text);
                allFoundFiles = Filef.GetFilesByExtensions(dirInfo, ".exe", ".lnk").ToList();
                for (int i = 0; i < allFoundFiles.Count; i++)
                {
                    if (allFoundFiles[i].Extension == ".lnk")
                    {
                        allFoundFiles[i] = new FileInfo(Filef.lnk(allFoundFiles[i]));

                        find = false;
                        int index = 0;
                        for (int j = 0; j < pro.Count; j++)
                        {
                            if (pro[j].path == allFoundFiles[i].FullName)
                            {
                                find = true;
                                index = j;
                            }
                        }
                        if (find) listBox1.Items.Add(allFoundFiles[i].Name + " Запусков программы: " + pro[index].count);
                        else listBox1.Items.Add(allFoundFiles[i].Name);
                    }
                    else
                    {
                        listBox1.Items.Add(allFoundFiles[i].Name);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Ошибка при запуске программы");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            string dir = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string serializationFile = Path.Combine(dir, "program.bin");
            using (Stream stream = System.IO.File.Open(serializationFile, FileMode.OpenOrCreate))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                bformatter.Serialize(stream, pro);
            }
        }
    }



    public static class Filef
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        public static string lnk(FileInfo linkf)
        {
            if (System.IO.File.Exists(linkf.FullName))
            {
                WshShell shell = new WshShell();
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(linkf.FullName);

                return(link.TargetPath);
            }
            MessageBox.Show("error");
            return linkf.ToString();
        }
    }
}
