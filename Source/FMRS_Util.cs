﻿/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014 SIT89
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace FMRS
{
    public class FMRS_Util : MonoBehaviour
    {
        private string mod_version;
        public const string gamesave_name = "FMRS_save_";

        public struct recover_value
        {
            public string cat;
            public string key;
            public string value;
        }

        public enum save_cat : int { SETTING = 1, SAVE, SAVEFILE, DROPPED, NAME, LANDED, DESTROYED, RECOVERED, KERBAL_DROPPED, UNDEF };

        public List<Guid> Vessels = new List<Guid>();
        public Dictionary<Guid, string> Vessels_dropped = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Vessels_dropped_names = new Dictionary<Guid, string>();
        public List<Guid> Vessels_dropped_landed = new List<Guid>();
        public List<Guid> Vessels_dropped_destroyed = new List<Guid>();
        public List<Guid> Vessels_dropped_recovered = new List<Guid>();
        public Dictionary<String, Guid> Kerbal_dropped = new Dictionary<string, Guid>();
        public List<recover_value> recover_values = new List<recover_value>();
        public bool Debug_Active = true, bflush_save_file = false;
        public bool Debug_Level_1_Active = false, Debug_Level_2_Active = false;
        public Dictionary<save_cat, Dictionary<string, string>> Save_File_Content = new Dictionary<save_cat, Dictionary<string, string>>();
        public Rect windowPos;
        public Guid _SAVE_Main_Vessel;
        public string _SAVE_Switched_To_Savefile;
        public bool _SETTING_Enabled, _SETTING_Armed, _SETTING_Auto_Cut_Off, _SETTING_Auto_Recover, _SETTING_Minimize, _SETTING_Throttle_Log, _SAVE_Has_Launched, _SAVE_Has_Closed, _SAVE_Kick_To_Main, _SAVE_Switched_To_Dropped;

        public string mod_vers
        {
            get { return mod_version; }
            set { mod_version = value; }
        }

/*************************************************************************************************************************/
        public void set_save_value(save_cat cat, string key, string value)
        {
            if (Debug_Level_2_Active)
                Debug.Log("#### FMRS: entering set_save_value(int cat, string key, string value)");

            if (Save_File_Content[cat].ContainsKey(key))
            {
                Save_File_Content[cat][key] = value;
            }
            else
            {
                Save_File_Content[cat].Add(key, value);
            }

            if (Debug_Active)
                Debug.Log("#### FMRS: set_save_value: " + key + " = " + value);

            if (Debug_Level_2_Active)
                Debug.Log("#### FMRS: leaving set_save_value(int cat, string key, string value)");
        }


/*************************************************************************************************************************/
        public string get_save_value(save_cat cat, string key)
        {
            if (Debug_Level_2_Active)
                Debug.Log("#### FMRS: entering get_save_value(int cat, string key) #### FMRS: NO LEAVE MESSAGE");

            if (Save_File_Content[cat].ContainsKey(key))
                return (Save_File_Content[cat][key]);
            else
                return (false.ToString());
        }


/*************************************************************************************************************************/
        public void write_save_values_to_file()
        {
            Vessel dummy_vessel = null;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering write_save_values_to_file()");

            set_save_value(save_cat.SETTING, "Window_X", Convert.ToInt32(windowPos.x).ToString());
            set_save_value(save_cat.SETTING, "Window_Y", Convert.ToInt32(windowPos.y).ToString());
            set_save_value(save_cat.SETTING, "Armed", _SETTING_Armed.ToString());
            set_save_value(save_cat.SETTING, "Minimized", _SETTING_Minimize.ToString());
            set_save_value(save_cat.SETTING, "Enabled", _SETTING_Enabled.ToString());
            set_save_value(save_cat.SETTING, "Auto_Cut_Off", _SETTING_Auto_Cut_Off.ToString());
            set_save_value(save_cat.SETTING, "Auto_Recover", _SETTING_Auto_Recover.ToString());
            set_save_value(save_cat.SETTING, "Throttle_Log", _SETTING_Throttle_Log.ToString());
            set_save_value(save_cat.SETTING, "Debug", Debug_Active.ToString());
            set_save_value(save_cat.SAVE, "Main_Vessel", _SAVE_Main_Vessel.ToString());
            set_save_value(save_cat.SAVE, "Has_Launched", _SAVE_Has_Launched.ToString());
            set_save_value(save_cat.SAVE, "Has_Closed", _SAVE_Has_Closed.ToString());
            set_save_value(save_cat.SAVE, "Kick_To_Main", _SAVE_Kick_To_Main.ToString());
            set_save_value(save_cat.SAVE, "Switched_To_Dropped", _SAVE_Switched_To_Dropped.ToString());
            set_save_value(save_cat.SAVE, "Switched_To_Savefile", _SAVE_Switched_To_Savefile.ToString());

            write_vessel_dict_to_Save_File_Content();
            
            TextWriter file = File.CreateText<FMRS>("save.txt", dummy_vessel);
            file.Flush();
            file.Close();
            file = File.CreateText<FMRS>("save.txt", dummy_vessel);
            foreach (KeyValuePair<save_cat, Dictionary<string, string>> save_cat_block in Save_File_Content)
            {
                foreach (KeyValuePair<string, string> writevalue in save_cat_block.Value)
                {
                   file.WriteLine(save_cat_block.Key.ToString() + "=" + writevalue.Key + "=" + writevalue.Value);
                }
            }
            file.Close();

            if (Debug_Active)
                Debug.Log("#### FMRS: Save File written in private void write_save_values_to_file()");
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving save_values_to_file()");
        }


/*************************************************************************************************************************/
        public void write_vessel_dict_to_Save_File_Content()
        {
            List<string> delete_values = new List<string>();
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering write_vessel_dict_to_Save_File_Content()");

            Save_File_Content[save_cat.DROPPED].Clear();
            Save_File_Content[save_cat.NAME].Clear();
            Save_File_Content[save_cat.LANDED].Clear();
            Save_File_Content[save_cat.DESTROYED].Clear();
            Save_File_Content[save_cat.RECOVERED].Clear();
            
            foreach (KeyValuePair<Guid, string> write_keyvalue in Vessels_dropped)
            {
                set_save_value(save_cat.DROPPED, write_keyvalue.Key.ToString(), write_keyvalue.Value);
                if (Debug_Active)
                    Debug.Log("#### FMRS: write " + write_keyvalue.Key.ToString() + " to Save_File_Content");
            }

            foreach (KeyValuePair<Guid, string> write_keyvalue in Vessels_dropped_names)
            {
                set_save_value(save_cat.NAME, write_keyvalue.Key.ToString(), write_keyvalue.Value);
                if (Debug_Active)
                    Debug.Log("#### FMRS: write NAME " + write_keyvalue.Key.ToString() + " to Save_File_Content");
            }

            foreach (Guid id  in Vessels_dropped_landed)
            {
                set_save_value(save_cat.LANDED, id.ToString(),true.ToString());
                if (Debug_Active)
                    Debug.Log("#### FMRS: write LANDED " + id.ToString() + " to Save_File_Content");
            }

            foreach (Guid id in Vessels_dropped_destroyed)
            {
                set_save_value(save_cat.DESTROYED, id.ToString(), true.ToString());
                if (Debug_Active)
                    Debug.Log("#### FMRS: write DESTROYED " + id.ToString() + " to Save_File_Content");
            }

            foreach (Guid id in Vessels_dropped_recovered)
            {
                set_save_value(save_cat.RECOVERED, id.ToString(), true.ToString());
                if (Debug_Active)
                    Debug.Log("#### FMRS: write RECOVERED " + id.ToString() + " to Save_File_Content");
            }

            foreach (KeyValuePair<string, Guid> write_keyvalue in Kerbal_dropped)
            {
                set_save_value(save_cat.KERBAL_DROPPED, write_keyvalue.Key.ToString(), write_keyvalue.Value.ToString());
                if (Debug_Active)
                    Debug.Log("#### FMRS: write KERBAL_DROPPED " + write_keyvalue.Key.ToString() + " to Save_File_Content");
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving write_vessel_dict_to_Save_File_Content()");
        }


/*************************************************************************************************************************/
        public string get_time_string(double dtime)
        {
            Int16 s, min, ds;
            Int32 time;
            string return_string;

            dtime *= 100;

            time = Convert.ToInt32(dtime);

            s = 0;
            ds = 0;
            min = 0;

            if (time >= 6000)
            {
                while (time >= 6000)
                {
                    min++;
                    time -= 6000;
                }
            }

            if (time >= 100)
            {
                while (time >= 100)
                {
                    s++;
                    time -= 100;
                }
            }

            if (time >= 10)
            {
                while (time >= 10)
                {
                    ds++;
                    time -= 10;
                }
            }

            if (min < 10)
                return_string = "0" + min.ToString();
            else
                return_string = min.ToString();

            return_string += ":";

            if (s < 10)
                return_string += "0" + s.ToString();
            else
                return_string += s.ToString();

            return_string += "." + ds.ToString();

            return (return_string);
        }


/*************************************************************************************************************************/
        public void flush_save_file()
        {
            Vessel dummy_vessel = null;
            int anz_lines;
            
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter flush_save_file()");

            if (Debug_Active)
                Debug.Log("#### FMRS: flush save file");

            string[] lines = File.ReadAllLines<FMRS>("save.txt", dummy_vessel);
            anz_lines = lines.Length;

            if (Debug_Active)
                Debug.Log("#### FMRS: delete " + anz_lines.ToString() + " lines");

            TextWriter file = File.CreateText<FMRS>("save.txt", dummy_vessel);
            while (anz_lines != 0)
            {
                file.WriteLine("");
                anz_lines--;
            }
            file.Close();

            foreach (KeyValuePair<save_cat, Dictionary<string, string>> content in Save_File_Content)
                Save_File_Content[content.Key].Clear();

            bflush_save_file = false;
            init_save_file();
            read_save_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave flush_save_file()");
        }


/*************************************************************************************************************************/
        public void read_save_file()
        {
            Vessel dummy_vessel = null;
            double temp_double;
            save_cat temp_cat;

            if (Debug_Level_1_Active) 
                Debug.Log("#### FMRS: enter read_save_file()");
				
			if (Debug_Active)
				Debug.Log("#### FMRS: read save file");

            foreach(KeyValuePair<save_cat,Dictionary<string,string>> content in Save_File_Content)
                Save_File_Content[content.Key].Clear();

            string[] lines = File.ReadAllLines<FMRS>("save.txt", dummy_vessel);

            foreach (string value_string in lines)
            {
                if (value_string != "")
                {
                    string[] line = value_string.Split('=');
                    temp_cat = save_cat_parse(line[0]);

                    try
                    {
                        Save_File_Content[temp_cat].Add(line[1].Trim(), line[2].Trim());
                    }
                    catch (Exception)
                    {
                        Debug_Active = true;
                        Debug_Level_1_Active = true;
                        Debug.Log("#### FMRS: inconsistent save file, flush save file");
                        bflush_save_file = true;
                        break;
                    }
                }
            }
            if (bflush_save_file)
                return;

            try
            {
                Debug_Active = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Debug"));

                temp_double = Convert.ToDouble(get_save_value(save_cat.SETTING, "Window_X"));
                windowPos.x = Convert.ToInt32(temp_double);
                temp_double = Convert.ToDouble(get_save_value(save_cat.SETTING, "Window_Y"));
                windowPos.y = Convert.ToInt32(temp_double);
            }
            catch (Exception)
            {
                Debug_Active = true;
                Debug_Level_1_Active = true;
                Debug.Log("#### FMRS: invalid save file, flush save file");
                bflush_save_file = true;
            }

            if (Debug_Active)
                foreach (KeyValuePair<save_cat, Dictionary<string, string>> temp_keyvalue in Save_File_Content)
                    foreach (KeyValuePair<string, string> readvalue in temp_keyvalue.Value)
                        Debug.Log("#### FMRS: " + temp_keyvalue.Key.ToString() + " = " + readvalue.Key + " = " + readvalue.Value);

            if (bflush_save_file)
                return;

            if (get_save_value(save_cat.SETTING,"Debug_Level") == "1" && Debug_Active)
                Debug_Level_1_Active = true;

            if (get_save_value(save_cat.SETTING,"Debug_Level") == "2" && Debug_Active)
            {
                Debug_Level_1_Active = true;
                Debug_Level_2_Active = true;
            }

            try
            {
                _SETTING_Armed = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Armed"));
                _SETTING_Minimize = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Minimized"));
                _SETTING_Enabled = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Enabled"));
                _SETTING_Auto_Cut_Off = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Auto_Cut_Off"));
                _SETTING_Auto_Recover = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Auto_Recover"));
                _SETTING_Throttle_Log = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Throttle_Log"));
                _SAVE_Main_Vessel = new Guid(get_save_value(save_cat.SAVE, "Main_Vessel"));
                _SAVE_Has_Launched = Convert.ToBoolean(get_save_value(save_cat.SAVE, "Has_Launched"));
                _SAVE_Has_Closed = Convert.ToBoolean(get_save_value(save_cat.SAVE, "Has_Closed"));
                _SAVE_Kick_To_Main = Convert.ToBoolean(get_save_value(save_cat.SAVE, "Kick_To_Main"));
                _SAVE_Switched_To_Dropped = Convert.ToBoolean(get_save_value(save_cat.SAVE, "Switched_To_Dropped"));
                _SAVE_Switched_To_Savefile = get_save_value(save_cat.SAVE, "Switched_To_Savefile");
            }
            catch (Exception)
            {
                Debug_Active = true;
                Debug_Level_1_Active = true;
                Debug.Log("#### FMRS: invalid save file, flush save file");
                bflush_save_file = true;
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave read_save_file()");
        }


/*************************************************************************************************************************/
        public void init_save_file()
        {
            Vessel dummy_vessel = null;
            
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter init_save_file()");

            if (Debug_Active)
                Debug.Log("#### FMRS: init save file");

            foreach (KeyValuePair<save_cat, Dictionary<string, string>> content in Save_File_Content)
                Save_File_Content[content.Key].Clear();

            set_save_value(save_cat.SETTING,"Version", mod_version);
            set_save_value(save_cat.SETTING, "Window_X", Convert.ToInt32(windowPos.x).ToString());
            set_save_value(save_cat.SETTING, "Window_Y", Convert.ToInt32(windowPos.y).ToString());
            set_save_value(save_cat.SETTING, "Enabled", true.ToString());
            set_save_value(save_cat.SETTING, "Armed", true.ToString());
            set_save_value(save_cat.SETTING, "Minimized", false.ToString());
            set_save_value(save_cat.SETTING, "Auto_Cut_Off", false.ToString());
            set_save_value(save_cat.SETTING, "Auto_Recover", false.ToString());
            set_save_value(save_cat.SETTING, "Throttle_Log", true.ToString());
            set_save_value(save_cat.SETTING, "Debug", false.ToString());
            set_save_value(save_cat.SETTING, "Debug_Level", "0");
            set_save_value(save_cat.SAVE, "Main_Vessel", new Guid().ToString());
            set_save_value(save_cat.SAVE, "Launched_At", "null");
            set_save_value(save_cat.SAVE, "Has_Launched", false.ToString());
            set_save_value(save_cat.SAVE, "Has_Closed", false.ToString());
            set_save_value(save_cat.SAVE, "Switched_To_Dropped", false.ToString());
            set_save_value(save_cat.SAVE, "Kick_To_Main", false.ToString());
            set_save_value(save_cat.SAVE, "Switched_To_Savefile", "");
           

#if DEBUG
            set_save_value(save_cat.SETTING,"Debug", true.ToString());
            set_save_value(save_cat.SETTING, "Debug_Level", "0");
#endif

            Debug_Active = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Debug"));

            TextWriter file = File.CreateText<FMRS>("save.txt", dummy_vessel);

            foreach (KeyValuePair<save_cat, Dictionary<string, string>> writecat in Save_File_Content)
                foreach (KeyValuePair<string, string> writevalue in writecat.Value)
                    file.WriteLine(writecat.Key.ToString() + "=" + writevalue.Key + "=" + writevalue.Value);

            file.Close();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave init_save_file()");
        }


/*************************************************************************************************************************/
        public void load_save_file()
        {
            Vessel dummy_vessel = null;

            if (!File.Exists<FMRS>("save.txt", dummy_vessel))
                init_save_file();
            read_save_file();

            if (!bflush_save_file)
                if (get_save_value(save_cat.SETTING, "Version") != mod_version)
                {
                    Debug.Log("#### FMRS: diferent version, flush save file");
                    bflush_save_file = true;
                }
            if (bflush_save_file)
                flush_save_file();

            if (!File.Exists<FMRS>("recover.txt", dummy_vessel))
                init_recover_file();
            read_recover_file();
        }


/*************************************************************************************************************************/
        public void get_dropped_vessels()
        {
             if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering get_dropped_vessels()");

            foreach (KeyValuePair<save_cat, Dictionary<string, string>> savecat in Save_File_Content)
            {
                if (savecat.Key == save_cat.DROPPED)
                    foreach(KeyValuePair<string,string> save_value in savecat.Value)
                    {
                        Vessels_dropped.Add(new Guid(save_value.Key), save_value.Value);

                        if (Debug_Active)
                            Debug.Log(" #### FMRS: " + save_value.Key.ToString() + " set to " + save_value.Value + " in Vessels_dropped");
                    }

                if (savecat.Key == save_cat.NAME)
                    foreach (KeyValuePair<string, string> save_value in savecat.Value)
                    {
                        Vessels_dropped_names.Add(new Guid(save_value.Key), save_value.Value);

                        if (Debug_Active)
                            Debug.Log(" #### FMRS: " + save_value.Key.ToString() + " set to " + save_value.Value + " in Vessels_dropped_names");
                    }

                if (savecat.Key == save_cat.LANDED)
                    foreach (KeyValuePair<string, string> save_value in savecat.Value)
                    {
                        Vessels_dropped_landed.Add(new Guid(save_value.Key));

                        if (Debug_Active)
                            Debug.Log(" #### FMRS: " + save_value.Key.ToString() + " set to " + save_value.Value + " in Vessels_dropped_landed");
                    }

                if (savecat.Key == save_cat.DESTROYED)
                    foreach (KeyValuePair<string, string> save_value in savecat.Value)
                    {
                        Vessels_dropped_destroyed.Add(new Guid(save_value.Key));

                        if (Debug_Active)
                            Debug.Log(" #### FMRS: " + save_value.Key + " set to " + save_value.Value + " in Vessels_dropped_destroyed");
                    }

                if (savecat.Key == save_cat.RECOVERED)
                    foreach (KeyValuePair<string, string> save_value in savecat.Value)
                    {
                        Vessels_dropped_recovered.Add(new Guid(save_value.Key));

                        if (Debug_Active)
                            Debug.Log(" #### FMRS: " + save_value.Key + " set to " + save_value.Value + " in Vessels_dropped_recovered");
                    }

                if (savecat.Key == save_cat.KERBAL_DROPPED)
                    foreach (KeyValuePair<string, string> save_value in savecat.Value)
                    {
                        Kerbal_dropped.Add(save_value.Key, new Guid(save_value.Value));

                        if (Debug_Active)
                            Debug.Log(" #### FMRS: " + save_value.Key + " set to " + save_value.Value + " in Kerbal_dropped");
                    }
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving get_dropped_vessels()");
        }


/*************************************************************************************************************************/
        public void init_recover_file()
        {
            Vessel dummy_vessel = null;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter init_recover_file()");
            if (Debug_Active)
                Debug.Log("#### FMRS: init recover file");

            TextWriter file = File.CreateText<FMRS>("recover.txt", dummy_vessel);
            file.Close();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave init_recover_file()");
        }


/*************************************************************************************************************************/
        public void flush_recover_file()
        {
            int anz_lines;
            Vessel dummy_vessel = null;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter flush_recover_file()");
            if (Debug_Active)
                Debug.Log("#### FMRS: flush recover file");

            string[] lines = File.ReadAllLines<FMRS>("recover.txt", dummy_vessel);
            anz_lines = lines.Length;

            TextWriter file = File.CreateText<FMRS>("recover.txt", dummy_vessel);
            while (anz_lines != 0)
            {
                file.WriteLine("");
                anz_lines--;
            }
            file.Close();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave flush_recover_file()");
        }


/*************************************************************************************************************************/
        public void read_recover_file()
        {
            Vessel dummy_vessel = null;
            recover_value temp_value;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter read_recover_file()");
            if (Debug_Active)
                Debug.Log("#### FMRS: read recover file");

            recover_values.Clear();
            string[] lines = File.ReadAllLines<FMRS>("recover.txt", dummy_vessel);

            foreach (string value_string in lines)
            {
                if (value_string != "")
                {
                    string[] line = value_string.Split('=');
                    temp_value.cat = line[0].Trim();
                    temp_value.key = line[1].Trim();
                    temp_value.value = line[2].Trim();
                    recover_values.Add(temp_value);
                }
            }

            if (Debug_Active)
                foreach (recover_value temp in recover_values)
                    Debug.Log("#### FMRS: recover value: " + temp.cat + " = " + temp.key + " = " + temp.value);

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave read_recover_file()");
        }


/*************************************************************************************************************************/
        public void write_recover_file()
        {
            Vessel dummy_vessel = null;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter write_recover_file()");
            if (Debug_Active)
                Debug.Log("#### FMRS: write recover file");

            flush_recover_file();

            TextWriter file = File.CreateText<FMRS>("recover.txt", dummy_vessel);

            foreach (recover_value writevalue in recover_values)
            {
                file.WriteLine(writevalue.cat + "=" + writevalue.key + "=" + writevalue.value);
            }
            file.Close();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: write_recover_file()");
        }


/*************************************************************************************************************************/
        public void set_recoverd_value(string cat, string key, string value)
        {
            recover_value temp_value;

            if (Debug_Level_2_Active)
                Debug.Log("#### FMRS: enter set_recoverd_value(string key, string value)");
            if (Debug_Active)
                Debug.Log("#### FMRS: add to recover file: " + cat + " = " + key + " = " + value);

            temp_value.cat = cat;
            temp_value.key = key;
            temp_value.value = value;
            recover_values.Add(temp_value);

            if (Debug_Level_2_Active)
                Debug.Log("#### FMRS: set_recoverd_value(string key, string value)");
        }


/*************************************************************************************************************************/
        public save_cat save_cat_parse(string in_string)
        {
            save_cat tempcat;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: enter save_cat_parse(string in_string) " + in_string);

            switch (in_string)
            {
                case "SETTING":
                    tempcat = save_cat.SETTING;
                    break;
                case "SAVE":
                    tempcat = save_cat.SAVE;
                    break;
                case "SAVEFILE":
                    tempcat = save_cat.SAVEFILE;
                    break;
                case "DROPPED":
                    tempcat = save_cat.DROPPED;
                    break;
                case "NAME":
                    tempcat = save_cat.NAME;
                    break;
                case "LANDED":
                    tempcat = save_cat.LANDED;
                    break;
                case "DESTROYED":
                    tempcat = save_cat.DESTROYED;
                    break;
                case "RECOVERED":
                    tempcat = save_cat.RECOVERED;
                    break;
                case "KERBAL_DROPPED":
                    tempcat = save_cat.KERBAL_DROPPED;
                    break;

                default:
                    tempcat = save_cat.UNDEF;
                    break;
            }

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leave save_cat_parse(string in_string)");
            return tempcat;
        }

        
/*************************************************************************************************************************/
        public void delete_dropped_vessels()
        {
            List<string> temp_list = new List<string>();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering delete_dropped_vessels()");

            Vessels_dropped.Clear();
            Vessels_dropped_names.Clear();
            Vessels_dropped_landed.Clear();
            Vessels_dropped_destroyed.Clear();
            Vessels_dropped_recovered.Clear();
            Kerbal_dropped.Clear();
            Save_File_Content[save_cat.SAVEFILE].Clear();

            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving delete_dropped_vessels()");
        }


/*************************************************************************************************************************/
        public void delete_dropped_vessel(Guid vessel_guid)
        {
            List<string> temp_list = new List<string>();
            string temp_string = null;

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering delete_dropped_vessel(Guid vessel_guid) " + vessel_guid.ToString());

            if (Debug_Active)
                Debug.Log("#### FMRS: remove vessel" + vessel_guid.ToString());

            if (Vessels_dropped.ContainsKey(vessel_guid))
                Vessels_dropped.Remove(vessel_guid);
            if (Vessels_dropped_names.ContainsKey(vessel_guid))
                Vessels_dropped_names.Remove(vessel_guid);
            if (Vessels_dropped_landed.Contains(vessel_guid))
                Vessels_dropped_landed.Remove(vessel_guid);
            if (Vessels_dropped_destroyed.Contains(vessel_guid))
                Vessels_dropped_destroyed.Remove(vessel_guid);
            if (Vessels_dropped_recovered.Contains(vessel_guid))
                Vessels_dropped_recovered.Remove(vessel_guid);

            foreach (KeyValuePair<string, Guid> Kerbal in Kerbal_dropped)
                if (Kerbal.Value == vessel_guid)
                    temp_string = Kerbal.Key;
            if(temp_string!=null)
                Kerbal_dropped.Remove(temp_string);
 

            write_save_values_to_file();

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving delete_dropped_vessel(Guid vessel_guid)");
        }


/*************************************************************************************************************************/
        public void init_Save_File_Content()
        {
            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: entering init_Save_File_Content()");

            if (!Save_File_Content.ContainsKey(save_cat.SETTING))
                Save_File_Content.Add(save_cat.SETTING, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.SAVE))
                Save_File_Content.Add(save_cat.SAVE, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.SAVEFILE))
                Save_File_Content.Add(save_cat.SAVEFILE, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.DROPPED))
                Save_File_Content.Add(save_cat.DROPPED, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.NAME))
                Save_File_Content.Add(save_cat.NAME, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.LANDED))
                Save_File_Content.Add(save_cat.LANDED, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.DESTROYED))
                Save_File_Content.Add(save_cat.DESTROYED, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.RECOVERED))
                Save_File_Content.Add(save_cat.RECOVERED, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.KERBAL_DROPPED))
                Save_File_Content.Add(save_cat.KERBAL_DROPPED, new Dictionary<string, string>());

            if (Debug_Level_1_Active)
                Debug.Log("#### FMRS: leaving init_Save_File_Content()");
        }
    }
}