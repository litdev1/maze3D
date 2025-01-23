using Microsoft.CSharp;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace plc3D
{
    public class Compiler
    {
        public MethodInfo methodSetup = null;
        public MethodInfo methodLoop = null;

        public bool Compile(string source)
        {
            methodSetup = null;
            methodLoop = null;
            try
            {
                CompilerParameters compilerParams = SetReferences();
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerResults compile = provider.CompileAssemblyFromSource(compilerParams, source);
                Module[] modules = compile.CompiledAssembly.GetModules();
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
                foreach (Module module in modules)
                {
                    Type[] types = module.GetTypes();
                    {
                        foreach (Type type in types)
                        {
                            MethodInfo[] methodInfos = type.GetMethods(bindingFlags);
                            foreach (MethodInfo methodInfo in methodInfos)
                            {
                                if (methodInfo.Name == "setup")
                                {
                                    methodSetup = methodInfo;
                                }
                                if (methodInfo.Name == "loop")
                                {
                                    methodLoop = methodInfo;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error compiling PLC code", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return (null != methodSetup && null != methodLoop);
        }

        private CompilerParameters SetReferences()
        {
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.GenerateExecutable = false;
            compilerParams.CompilerOptions = "/optimize";
            compilerParams.GenerateInMemory = true;

            List<string> loadedAssemblies = new List<string>();

            AddReferences(Assembly.GetExecutingAssembly(), compilerParams);
            //foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    AddReferences(assembly, compilerParams);
            //}

            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Resources"))
            {
                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Resources");
                foreach (string file in files)
                {
                    compilerParams.EmbeddedResources.Add(file);
                }
            }

            return compilerParams;
        }

        private void AddReferences(Assembly assembly, CompilerParameters compilerParams)
        {
            AddReference(assembly, compilerParams);
            foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
            {
                try
                {
                    AddReference(Assembly.ReflectionOnlyLoad(assemblyName.FullName), compilerParams);
                }
                catch
                {
                }
            }
        }

        private void AddReference(Assembly assembly, CompilerParameters compilerParams)
        {
            compilerParams.ReferencedAssemblies.Add(assembly.Location);
        }
    }
}
