using System;
using System.Linq;
using System.Collections.Generic;
using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper;
using UnityEngine;

namespace SoundTest
{
    public class SoundTest : ModBehaviour
    {
        readonly List<IModButton> hiddenButtons = new List<IModButton>();
        bool uiOpen;
        Vector2 scrollPosition;
        SortMode sortMode;

        private void Start()
        {
            ModHelper.Console.WriteLine($"{nameof(SoundTest)} is loaded!", MessageType.Success);
            ModHelper.Menus.PauseMenu.OnInit += PauseMenu_OnInit;
            ModHelper.Menus.PauseMenu.OnClosed += PauseMenu_OnClosed;
        }

        private void PauseMenu_OnInit()
        {
            var openFactLogBtn = ModHelper.Menus.PauseMenu.OptionsButton.Duplicate("SOUND TEST");
            openFactLogBtn.OnClick += () =>
            {
                ToggleUI();
            };
        }

        private void PauseMenu_OnClosed()
        {
            if (uiOpen) ToggleUI();
        }

        private void ToggleUI()
        {
            if (uiOpen)
            {
                uiOpen = false;
                foreach (var btn in hiddenButtons)
                {
                    btn.Show();
                }
                hiddenButtons.Clear();
            }
            else
            {
                uiOpen = true;
                foreach (var btn in ModHelper.Menus.PauseMenu.Buttons)
                {
                    if (btn.Button.gameObject.activeSelf)
                    {
                        btn.Hide();
                        hiddenButtons.Add(btn);
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!uiOpen) return;

            sortMode = (SortMode)GUILayout.SelectionGrid((int)sortMode, Enum.GetNames(typeof(SortMode)), 2);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);
            GUILayout.BeginVertical();
            if (sortMode == SortMode.AudioType)
            {
                var types = Enum.GetValues(typeof(AudioType)).Cast<AudioType>().OrderBy(t => Enum.GetName(typeof(AudioType), t));
                foreach (var type in types)
                {
                    if (GUILayout.Button(Enum.GetName(typeof(AudioType), type)))
                    {
                        var clip = Locator.GetAudioManager().GetSingleAudioClip(type);
                        Locator.GetMenuAudioController()._audioSource.PlayOneShot(clip);
                    }
                }
            }
            else if (sortMode == SortMode.AudioClip)
            {
                var clips = Locator.GetAudioManager()._libraryAsset.audioEntries.SelectMany(e => e.clips).OrderBy(c => c.name);
                foreach (var clip in clips)
                {
                    if (GUILayout.Button(clip.name))
                    {
                        Locator.GetMenuAudioController()._audioSource.PlayOneShot(clip);
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        public enum SortMode
        {
            AudioType,
            AudioClip,
        }
    }
}
