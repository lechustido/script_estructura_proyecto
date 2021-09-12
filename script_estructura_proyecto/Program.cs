using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace script_estructura_proyecto



//Todo. al importar, hay que ver de conde carajo savo el nombre del proyecto (seria bueno revisar la carpeta padre)
{
    class Program
    {
        public static string proyectName;
        public static string urlModels = Environment.CurrentDirectory + "\\Models";
        public static string urlControllers = Environment.CurrentDirectory + "\\Controllers";
        public static string urlMyModels = Environment.CurrentDirectory + "\\modelos\\";
        public static string urlInterfaces = Environment.CurrentDirectory + "\\Interfaces";
        public static string urlInfraestructure = Environment.CurrentDirectory + "\\Infraestructure";

        public static void Main(string[] args)
        {
            string[] directories = Environment.CurrentDirectory.Split('\\');
            proyectName = directories[directories.Length - 1];
            if (Directory.Exists(urlModels))
            {
                identifyModels(urlModels);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", urlModels);
            }
        }

        public static void identifyModels(string targetDirectory)
        {
            List<string> arrayModels = new List<string>();

            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                string fileNameTrim = fileName.Split("Models\\")[1];
                fileNameTrim = fileNameTrim.Split(".cs")[0];
                if (fileNameTrim != "DBContext")
                {
                    arrayModels.Add(fileNameTrim);
                }
            }

            createControllers(arrayModels);
            createInterfaces(arrayModels);
            createInfraestructure(arrayModels);
            createAutomapperFile(arrayModels);
            writeInFile(arrayModels);

        }

        #region CreatingDirectories
        public static void createControllers(List<string> arrayModels)
        {
            if (!Directory.Exists(urlControllers))
            {
                Directory.CreateDirectory("Controllers");
            }

            foreach (string fileName in arrayModels)
            {
                string filenameModel = Environment.CurrentDirectory + "\\modelos\\Controller.txt";
                string readText = File.ReadAllText(filenameModel);
                string filenameController = urlControllers + "\\" + fileName + "Controller.cs";
                fileWrite(fileName, readText, filenameController);
            }
        }

        public static void createInterfaces(List<string> arrayModels)
        {
            if (!Directory.Exists(urlInterfaces))
            {
                Directory.CreateDirectory("Interfaces");
            }

            foreach (string fileName in arrayModels)
            {
                if (!Directory.Exists(urlInterfaces + "\\" + fileName))
                {
                    Directory.CreateDirectory(urlInterfaces + "\\" + fileName);
                }

                string filenameModelService = urlMyModels + "\\IService.txt";
                string readTextService = File.ReadAllText(filenameModelService);
                string filenameControllerService = urlInterfaces + "\\" + fileName + "\\I" + fileName + "Service.cs";
                fileWrite(fileName, readTextService, filenameControllerService);

                string filenameModelRepository = Environment.CurrentDirectory + "\\modelos\\IRepository.txt";
                string readTextRepository = File.ReadAllText(filenameModelRepository);
                string filenameControllerRepository = urlInterfaces + "\\" + fileName + "\\I" + fileName + "Repository.cs";
                fileWrite(fileName, readTextRepository, filenameControllerRepository);
            }
        }

        public static void createInfraestructure(List<string> arrayModels)
        {
            if (!Directory.Exists(urlInfraestructure))
            {
                Directory.CreateDirectory("Infraestructure");
            }

            foreach (string fileName in arrayModels)
            {
                if (!Directory.Exists(urlInfraestructure + "\\" + fileName))
                {
                    Directory.CreateDirectory(urlInfraestructure + "\\" + fileName);
                }

                string filenameModelService = urlMyModels + "\\Service.txt";
                string readTextService = File.ReadAllText(filenameModelService);
                string filenameControllerService = urlInfraestructure + "\\" + fileName + "\\" + fileName + "Service.cs";
                fileWrite(fileName, readTextService, filenameControllerService);

                string filenameModelRepository = urlMyModels + "\\Repository.txt";
                string readTextRepository = File.ReadAllText(filenameModelRepository);
                string filenameControllerRepository = urlInfraestructure + "\\" + fileName + "\\" + fileName + "Repository.cs";
                fileWrite(fileName, readTextRepository, filenameControllerRepository);
            }
        }

        public static void createAutomapperFile(List<string> arrayModels)
        {

            foreach (string fileName in arrayModels)
            {
                if (!Directory.Exists(urlModels + "\\" + fileName))
                {
                    Directory.CreateDirectory(urlModels + "\\" + fileName);
                }

                string filenameModelService = urlMyModels + "\\AutoMapperProfile.txt";
                string readTextService = File.ReadAllText(filenameModelService);
                string filenameControllerService = urlModels + "\\" + fileName + "\\" + "AutoMapperProfile" + fileName + ".cs";
                fileWrite(fileName, readTextService, filenameControllerService);
            }
        }

        #endregion CreatingDirectories


        #region createfile
        public static void fileWrite(string fileName, string textModel, string newController)
        {
            using (FileStream fs = File.Create(newController))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(adaptNameFile(fileName, textModel));
                fs.Write(info, 0, info.Length);
            }

            using (StreamReader sr = File.OpenText(newController))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    Console.WriteLine(s);
                }
            }
        }

        public static string adaptNameFile(string filename, string fileTextModel)
        {
            string newText = fileTextModel.Replace("textoRemplazo", filename);

            // No se como hacerlo, opeero solo tengo    que coger el nombre de la carpeta padre, no todo el path
            string[] directories = Environment.CurrentDirectory.Split('\\');
            newText = newText.Replace("nombreProyectoRemplazo", directories[directories.Length - 1]);
            newText = newText.Replace("//", "");
            return newText;
        }
        #endregion createfile 

        #region writeInFile
        public static void writeInFile(List<string> arrayModels)
        {
            string starUpData = Environment.CurrentDirectory + "\\Startup.cs";
            string readTextService = File.ReadAllText(starUpData);

            foreach (string fileName in arrayModels)
            {
                // USING
                readTextService = readTextService.Insert(0, "using " + proyectName + ".Infraestructure." + fileName + "s;\n");
                readTextService = readTextService.Insert(0, "using " + proyectName + ".Interfaces." + fileName + "s;\n");
                readTextService = readTextService.Insert(0, "using " + proyectName + ".Models\n");
            }

            int indexOfConfigService = readTextService.IndexOf("public void ConfigureServices(IServiceCollection services)") + 75;

            foreach (string fileName in arrayModels)
            {
                // Añadido a configure service
                readTextService = readTextService.Insert(indexOfConfigService, "services.AddAutoMapper(typeof(AutoMapperProfile" + fileName + ").Assembly);\n");
                readTextService = readTextService.Insert(indexOfConfigService, "services.AddScoped<I" + fileName + "Repository," + fileName + "Repository > ();\n");
                readTextService = readTextService.Insert(indexOfConfigService, "services.AddScoped<I" + fileName + "Service," + fileName + "Service > ();\n");

                readTextService = readTextService.Insert(indexOfConfigService, "\n");
            }
            using (FileStream fs = File.Create(starUpData))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(readTextService);
                fs.Write(info, 0, info.Length);
            }

            using (StreamReader sr = File.OpenText(starUpData))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    Console.WriteLine(s);
                }
            }
        }

        #endregion writeInFile 
    }
}
