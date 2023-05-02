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
        readonly List<IModButton> hiddenButtons = new();
        bool uiOpen;
        Vector2 scrollPosition;
        SortMode sortMode;
        AudioType selectedAudioType;
        AudioClip selectedAudioClip;
        OWAudioSource audioSource;

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
                if (!audioSource)
                {
                    audioSource = Instantiate(Locator.GetMenuAudioController()._audioSource.gameObject).GetComponent<OWAudioSource>();
                    Destroy(audioSource.gameObject.GetComponent<MenuAudioController>());
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
                    if (selectedAudioType == type && audioSource.isPlaying)
                    {
                        if (!GUILayout.Toggle(true, Enum.GetName(typeof(AudioType), type), "button"))
                        {
                            selectedAudioType = AudioType.None;
                            selectedAudioClip = null;
                            audioSource.Stop();
                        }
                    } else if (GUILayout.Button(Enum.GetName(typeof(AudioType), type)))
                    {
                        audioSource.Stop();
                        selectedAudioType = type;
                        selectedAudioClip = null;
                        audioSource.AssignAudioLibraryClip(type);
                        audioSource.Play();
                    }
                }
            }
            else if (sortMode == SortMode.AudioClip)
            {
                var clips = Enum.GetValues(typeof(AudioType)).Cast<AudioType>().SelectMany(t => t == AudioType.None ? new AudioClip[] { } : Locator.GetAudioManager().GetAudioClipArray(t)).Where(c => c != null).OrderBy(c => c.name);
                foreach (var clip in clips)
                {
                    if (selectedAudioClip == clip && audioSource.isPlaying)
                    {
                        if (!GUILayout.Toggle(true, clip.name, "button"))
                        {
                            selectedAudioClip = null;
                            selectedAudioType = AudioType.None;
                            audioSource.Stop();
                        }
                    } else if (GUILayout.Button(clip.name))
                    {
                        audioSource.Stop();
                        selectedAudioClip = clip;
                        selectedAudioType = AudioType.None;
                        audioSource._audioLibraryClip = AudioType.None;
                        audioSource.clip = clip;
                        audioSource.Play();
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
