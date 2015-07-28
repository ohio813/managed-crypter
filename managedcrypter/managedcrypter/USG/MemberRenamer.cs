using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace managedcrypter.USG
{
    public class MemberRenamer
    {
        public static void RenameMembers(string filePath)
        {
            AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(filePath);
            asmDef.Name = new AssemblyNameDefinition(Utils.GenerateRandomString(8, 16), new Version(0, 0, 0, 0));
            asmDef.MainModule.Name = Utils.GenerateRandomString(8, 16);

            foreach (TypeDefinition tDef in asmDef.MainModule.GetTypes())
            {
                if (tDef.IsSpecialName || tDef.IsRuntimeSpecialName)
                    continue;

                if (tDef.IsClass)
                {
                    tDef.Name = Utils.GenerateRandomString(8, 16);

                    if (tDef.HasMethods)
                    {
                        foreach (MethodDefinition mtdDef in tDef.Methods)
                        {
                            if (mtdDef.IsPublic || mtdDef.IsPrivate || !mtdDef.IsSpecialName)
                            {

                                mtdDef.Name = Utils.GenerateRandomString(8, 16);

                                if (mtdDef.HasParameters)
                                {
                                    foreach (ParameterDefinition parDef in mtdDef.Parameters)
                                        parDef.Name = Utils.GenerateRandomString(8, 16);
                                }

                                if (mtdDef.HasBody)
                                {
                                    if (mtdDef.Body.HasVariables)
                                    {
                                        foreach (var varDef in mtdDef.Body.Variables)
                                            varDef.Name = Utils.GenerateRandomString(8, 16);
                                    }
                                }
                            }
                        }
                    }
                }

                if (tDef.HasFields)
                {
                    foreach (FieldDefinition fldDef in tDef.Fields)
                        fldDef.Name = Utils.GenerateRandomString(8, 16);
                }

                if (tDef.HasProperties)
                {
                    foreach (PropertyDefinition proDef in tDef.Properties)
                        proDef.Name = Utils.GenerateRandomString(8, 16);
                }
            }

            asmDef.Write(filePath);
        }
    }
}
