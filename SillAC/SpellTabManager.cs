using MyClasses.MetaViewWrappers.DecalControls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace SillAC
{
    internal class SpellTabManager
    {
        //Might be wrong, but in testing only up to tab 7 was supported
        private static int MAX_SPELL_TAB = 7;
        private static string DEFAULT_SPELL_PATH = "Spells.json";

        private Timer timer;
        private List<Spell> spellsToLoad = new List<Spell>();
        private int spellIndex { get; set; } = 0;
        private bool spellBarsEmpty = false;

        private void SpendExperienceTick(object sender, ElapsedEventArgs e)
        {
            //Check for cleared spell bars
            if(!spellBarsEmpty)
            {
                if (!IsSpellBarsEmpty())
                {
                    Util.WriteToChat("Waiting for spellbar to be emptied.");
                    return;
                }
                spellBarsEmpty = true;
            }

            //If there's no more spells stop
            if (spellsToLoad.Count <= spellIndex)
            {
                //Plan finished.
                Util.WriteToChat($"Finished loading {spellIndex} spells.");
                timer.Enabled = false;
            }
            else
            {
                var spell = spellsToLoad[spellIndex];
                Globals.Host.Actions.SpellTabAdd(spell.Tab, spell.Index, spell.SpellID);
                spellIndex++;
            }
        }

        private bool IsSpellBarsEmpty()
        {
            for(var i = 0; i < MAX_SPELL_TAB; i++)
            {
                if (Globals.Core.CharacterFilter.SpellBar(i).Count > 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Behavior when logging off.
        /// </summary>
        internal void Stop()
        {
            timer.Enabled = false;
            timer.Elapsed -= SpendExperienceTick;
        }
        public void Start(int interval)
        {
            timer = new Timer() { AutoReset = true, Enabled = false, Interval = interval };
            timer.Elapsed += SpendExperienceTick;
        }


        public SpellTabManager()
        {
        }

        public void SaveAllSpells()
        {
            SaveAllSpells(DEFAULT_SPELL_PATH);
        }
        public void SaveAllSpells(string path)
        {
            path = Path.GetFullPath(path);
            SpellTab[] tabs = new SpellTab[MAX_SPELL_TAB];
            for (var i = 0; i < tabs.Length; i++)
                tabs[i] = GetSpellTab(i);

            string json = JsonConvert.SerializeObject(tabs);

            try
            {
                File.WriteAllText(path, json);
                Util.WriteToChat($"Saved spells to {path}");
            }
            catch(Exception e)
            {
                Util.WriteToChat($"Unable to save spells to: {System.IO.Path.GetFullPath(path)}");
                Util.LogError(e);
            }
        }

        public SpellTab GetSpellTab(int tabNumber)
        {
            if (tabNumber < 0 || tabNumber >= MAX_SPELL_TAB)
                throw new Exception("Tried to get an illegal spell tab: " + tabNumber);

            return new SpellTab() { Spells = Globals.Core.CharacterFilter.SpellBar(tabNumber).ToArray() };
        }

        public void ClearAllSpells()
        {
            //Util.WriteToChat("Clearing all spell bars");
            for (var i = 0; i < MAX_SPELL_TAB; i++)
            {
                ClearTab(i);
            }
        }
        public void ClearTab(int tabNumber)
        {
            if(tabNumber < 0 || tabNumber >= MAX_SPELL_TAB)
                throw new Exception("Tried to clear an illegal spell tab: " + tabNumber);

            var tab = Globals.Core.CharacterFilter.SpellBar(tabNumber);
            //for (var i = tab.Count; i >= 0; i--)
            for (var i = 0; i < tab.Count; i++)
            {
                Globals.Host.Actions.SpellTabDelete(tabNumber, tab[i]);
            }
        }


        #region Load Spells Details
        // Summary:
        //     Adds a spell to the specified tab on the player's spell bar. The spell must be
        //     in the player's spell book. Each spell tab can contain only one copy of each
        //     spell. Putting a spell onto a tab that already contains that spell will just
        //     move the spell to the new index.
        //
        // Parameters:
        //   tab:
        //     The zero-based tab index to add the spell.
        //
        //   index:
        //     The zero-based slot on the tab to add the spell. If this index is greater than
        //     the number of spells on the tab, the spell will be added to the first unused
        //     slot.
        //
        //   spellId:
        //     The ID of the spell to be added.
        #endregion
        public void LoadSpells()
        {
            LoadSpells(DEFAULT_SPELL_PATH);
        }
        public void LoadSpells(string parameters)
        {
            //TODO: Wait until spells are all gone / implement something that only deletes/inserts what you need to
            ClearAllSpells();

            var path = Path.GetFullPath(parameters);
            try
            {
                var json = File.ReadAllText(path);
                var tabs = JsonConvert.DeserializeObject<SpellTab[]>(json);

                //Populate list of spells to be leveled
                spellsToLoad = new List<Spell>();
                for(var i = 0; i < tabs.Length; i++)
                {
                    var tab = tabs[i];
                    for(var j = 0; j < tab.Spells.Length; j++)
                    {
                        var spell = new Spell
                        {
                            Tab = i,
                            Index = j,
                            SpellID = tab.Spells[j]
                        };
                        spellsToLoad.Add(spell);
                        //Util.WriteToChat($"Planning to load ID {spell.SpellID} to tab {spell.Tab}, Slot {spell.Index}");
                    }
                }

                //Start leveling
                spellIndex = 0;
                timer.Enabled = true;

                Util.WriteToChat($"Loading {spellsToLoad.Count} spells from {path}");
            }
            catch (Exception ex)
            {
                Util.WriteToChat($"Failed to load spells from: {path}");
                Util.LogError(ex);

            }
        }
    }
}