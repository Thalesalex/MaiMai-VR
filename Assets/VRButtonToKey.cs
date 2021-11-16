﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using WindowsInput;

public class VRButtonToKey : MonoBehaviour
{
    [DllImport("user32.dll")]
    public static extern uint MapVirtualKey(uint uCode, uint uMapType);
    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    public static class WinAPI
    {
        public const uint MAPVK_VK_TO_SC = 0;
        public const uint MAPVK_SC_TO_VK = 1;

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();

        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;

        [StructLayout(LayoutKind.Explicit)]
        public struct Input
        {
            [FieldOffset(0)]
            public int Type;

            [FieldOffset(4)]
            public KeyboardInput Ki;

            [FieldOffset(4)]
            public MouseInput Mi;

            [FieldOffset(4)]
            public HardwareInput Hi;

            public static Input Keyboard(KeyboardInput input)
            {
                return new Input { Type = INPUT_KEYBOARD, Ki = input };
            }

            public static Input Mouse(MouseInput input)
            {
                return new Input { Type = INPUT_MOUSE, Mi = input };
            }

            public static Input Hardware(HardwareInput input)
            {
                return new Input { Type = INPUT_HARDWARE, Hi = input };
            }
        }

        public const uint KEYEVENTF_EXTENDED = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_SCANCODE = 0x0008;

        public struct KeyboardInput
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;

            public KeyboardInput(ushort Vk = 0, ushort Scan = 0, uint Flags = 0, uint Time = 0)
            {
                this.Vk = Vk;
                this.Scan = Scan;
                this.Flags = Flags;
                this.Time = Time;
                ExtraInfo = GetMessageExtraInfo();
            }
        }

        public struct MouseInput
        {
            public int Dx;
            public int Dy;
            public uint Data;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;

            public MouseInput(int Dx = 0, int Dy = 0, uint Data = 0, uint Flags = 0, uint Time = 0)
            {
                this.Dx = Dx;
                this.Dy = Dy;
                this.Data = Data;
                this.Flags = Flags;
                this.Time = Time;
                ExtraInfo = GetMessageExtraInfo();
            }
        }

        public struct HardwareInput
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;

            public HardwareInput(uint Msg = 0, ushort ParamL = 0, ushort ParamH = 0)
            {
                this.Msg = Msg;
                this.ParamL = ParamL;
                this.ParamH = ParamH;
            }
        }

        public static uint SendInput(Input[] inputs)
        {
            return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        }
    }
    public VirtualKeyCode keyToPress;
    public AudioClip click;
    public Light lightTarget;
    public AudioSource audioSource;
    public float frequency;
    public float amplitude;
    public OVRInput.Controller controllerMaskR = new OVRInput.Controller();
    public OVRInput.Controller controllerMaskL = new OVRInput.Controller();
    // Start is called before the first frame update
        void Start()
        {
        audioSource = transform.gameObject.AddComponent<AudioSource>();
        audioSource.clip = click;
        audioSource.volume = 0.8f;
        lightTarget.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.name);
        keybd_event(System.Convert.ToByte(keyToPress), (byte)MapVirtualKey((uint)keyToPress, 0), 0, UIntPtr.Zero);  
        audioSource.Play();
        lightTarget.gameObject.SetActive(true);
        if (other.gameObject.name=="OVRControllerPrefabR")
            OVRInput.SetControllerVibration(frequency, amplitude, controllerMaskR);
        else
            OVRInput.SetControllerVibration(frequency, amplitude, controllerMaskL);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name=="OVRControllerPrefabR")
            OVRInput.SetControllerVibration(frequency, amplitude, controllerMaskR);
        else
            OVRInput.SetControllerVibration(frequency, amplitude, controllerMaskL);
    }
    private void OnTriggerExit(Collider other)
    {
        
        keybd_event(System.Convert.ToByte(keyToPress), (byte)MapVirtualKey((uint)keyToPress, 0), 2, UIntPtr.Zero);
        lightTarget.gameObject.SetActive(false);
        if (other.gameObject.name=="OVRControllerPrefabR")
            OVRInput.SetControllerVibration(0f, 0f, controllerMaskR);
        else
            OVRInput.SetControllerVibration(0f, 0f, controllerMaskL);
    }

        public static void SendKeyEvent(uint keyCode, bool isDown)
        {
            uint flags = WinAPI.KEYEVENTF_SCANCODE;

            if (!isDown) flags |= WinAPI.KEYEVENTF_KEYUP;
            if ((keyCode & 0x100) > 0) flags |= WinAPI.KEYEVENTF_EXTENDED;

            ushort scan = (ushort)WinAPI.MapVirtualKey(keyCode & 0xFF, WinAPI.MAPVK_VK_TO_SC);

            WinAPI.SendInput(new WinAPI.Input[] {
                    WinAPI.Input.Keyboard(new WinAPI.KeyboardInput(Scan: scan, Flags: flags)),
                });
        }
}
