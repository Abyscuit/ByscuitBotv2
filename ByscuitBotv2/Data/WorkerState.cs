using ByscuitBotv2.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByscuitBotv2.Data
{
    public class WorkerState
    {
        public ulong lastshare { get; set; }
        public uint rating { get; set; }
        public uint prevShares { get; set; }
        public uint termShares { get; set; }

        public WorkerState()
        {
        }

        public WorkerState(ulong lstShr, uint Rating, uint PrvShr, uint svShr)
        {
            lastshare = lstShr;
            rating = Rating;
            prevShares = PrvShr;
            termShares = Rating - PrvShr;
        }
        public WorkerState(Nanopool.Worker worker)
        {
            lastshare = worker.lastshare;
            rating = worker.rating;
            prevShares = worker.prevShares;
            termShares = worker.rating - worker.prevShares;
        }
    }

    public class WorkerStates
    {
        public struct WorkerStateStruct
        {
            public string id { get; set; }
            public ulong uid { get; set; }
            public List<WorkerState> states;
            public WorkerStateStruct(Nanopool.Worker worker)
            {
                id = worker.id;
                uid = worker.uid;
                states = new List<WorkerState>();
                WorkerState workerState = new WorkerState(worker);
                states.Add(workerState);
            }

            public uint GetTotalShares()
            {
                uint total = 0;
                for (int i = 0; i < states.Count; i++)
                {
                    // If the share count is negative remove it from the saved states
                    if (states[i].rating < states[i].prevShares) { states.RemoveAt(i); Save(); continue; }
                    total += states[i].termShares;
                }
                return total;
            }

            public void AddNewState(Nanopool.Worker worker)
            {
                // Check if the new state is a duplicate before adding
                WorkerState state = new WorkerState(worker);
                if (states.Contains(state))
                {
                    Utility.printConsole($"New state for {worker.id} is a duplicate...");
                    Utility.printConsole($"Replacing latest worker state instead.");
                    ReplaceCurrentState(worker);
                    return;
                }
                states.Add(new WorkerState(worker));
                Save();
            }
            public void ReplaceCurrentState(Nanopool.Worker worker)
            {
                states.Remove(states.Last());
                states.Add(new WorkerState(worker));
                Save();
            }

            public WorkerState GetLastState()
            {
                return states.Last();
            }
        }
        public static List<WorkerStateStruct> states = new List<WorkerStateStruct>();

        public static WorkerStateStruct getWorkerStruct(Nanopool.Worker worker)
        {
            for (int i = 0; i < states.Count; i++)
            {
                WorkerStateStruct Struct = states[i];
                if (Struct.uid == worker.uid) return Struct;
            }
            WorkerStateStruct newStruct = new WorkerStateStruct(worker);
            states.Add(newStruct);
            Save();
            return states.Last();
        }

        public static void Save()
        {
            string path = "Resources/";
            string file = "WorkerStates.json";
            string fullpath = path + file;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllText(fullpath, JsonConvert.SerializeObject(states, Formatting.Indented));
        }

        public static void Load()
        {
            string path = "Resources/";
            string file = "WorkerStates.json";
            string fullpath = path + file;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string data = "";
            if (File.Exists(fullpath))
            {
                data = File.ReadAllText(fullpath);
                states = JsonConvert.DeserializeObject<List<WorkerStateStruct>>(data);
                Utility.printConsole("Loaded WorkerStates successfully!");
            }
        }

        public static void Reset()
        {
            states = new List<WorkerStateStruct>();
            Save();
            Utility.printConsole("WorkerStates have been reset");
        }
    }
}
