using RoR2;
using System;
using System.Collections.Generic;

namespace R2API.Utils {

    class ModProc {
        public Dictionary<string, bool> ProcList = new Dictionary<string, bool>();

        public ModProc() {
            foreach (var proc in (ProcType[])Enum.GetValues(typeof(ProcType))) {
                ProcList.Add(proc.ToString(), false);
            }
            foreach (var proc in ModProcManager.CustomProcList) {
                ProcList.Add(proc, false);
            }
        }

        public void ChangeProcState(string proc, bool value) {
            if (ProcList.ContainsKey(proc))
                ProcList[proc] = value;
        }
    }

    static class ProcChainMaskExtension {
        public static void SetProcValue(this ProcChainMask procMask, string procName, bool value) {
            ModProcManager.SetProcValue(procMask, procName, value);
        }
        public static void SetProcValue(this ProcChainMask procMask, ProcType proc, bool value) {
            ModProcManager.SetProcValue(procMask, proc.ToString(), value);
        }
        public static bool GetProcValue(this ProcChainMask procMask, string procName) {
            return ModProcManager.GetProcValue(procMask, procName);
        }
        public static bool GetProcValue(this ProcChainMask procMask, ProcType proc) {
            return ModProcManager.GetProcValue(procMask, proc.ToString());
        }
        public static void LinkToManager(this ProcChainMask procMask) {
            ModProcManager.AddLink(procMask, new ModProc());
        }
        public static void UnlinkToManager(this ProcChainMask procMask) {
            ModProcManager.RemoveLink(procMask);
        }
    }

    class ModProcManager {

        public static List<string> CustomProcList = new List<string>();

        /// <summary>
        /// Used to declare new Proc Type in addition to existing one
        /// </summary>
        /// <param name="procName"></param>
        public static void DeclareNewProc(string procName) {
            if (!CustomProcList.Contains(procName)) {
                CustomProcList.Add(procName);
            }
            else {
                throw new Exception("Mod Proc Manager : Trying to declare an existing Proc : " + procName);
            }
        }

        public static Dictionary<ProcChainMask, ModProc> ProcChainLinker = new Dictionary<ProcChainMask, ModProc>();

        public static void SetProcValue(ProcChainMask chain, string procName, bool value) {
            if (ProcChainLinker.ContainsKey(chain)) {
                ProcChainLinker[chain].ChangeProcState(procName, value);
            }
        }
        public static bool GetProcValue(ProcChainMask chain, string procName) {
            if (ProcChainLinker.ContainsKey(chain)) {
                if (ProcChainLinker[chain].ProcList.ContainsKey(procName))
                    return ProcChainLinker[chain].ProcList[procName];
            }
            return false;
        }

        public static void AddLink(ProcChainMask chain, ModProc modProc) {
            if (!ProcChainLinker.ContainsKey(chain)) {
                ProcChainLinker.Add(chain, modProc);
            }
        }

        public static void RemoveLink(ProcChainMask chain) {
            if (ProcChainLinker.ContainsKey(chain)) {
                ProcChainLinker.Remove(chain);
            }
        }

        public static void RemoveAllLink() {
            ProcChainLinker = new Dictionary<ProcChainMask, ModProc>();
        }


    }


}
