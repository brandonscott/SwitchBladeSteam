﻿/* ChatWindow.cs
 *
 * Copyright © 2013 by Adam Hellberg
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Disclaimer: SwitchBladeSteam is in no way affiliated
 * with Razer and/or any of its employees and/or licensors.
 * Adam Hellberg does not take responsibility for any harm caused, direct
 * or indirect, to any Razer peripherals via the use of SwitchBladeSteam.
 * 
 * "Razer" is a trademark of Razer USA Ltd.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Sharparam.SharpBlade.Logging;
using Sharparam.SharpBlade.Native;
using Sharparam.SharpBlade.Razer;
using Sharparam.SharpBlade.Razer.Events;
using Sharparam.SteamLib;
using Sharparam.SteamLib.Events;
using Steam4NET;

namespace Sharparam.SwitchBladeSteam.App
{
    public partial class ChatWindow : Form , IKeyboardEnabledForm
    {
        private delegate void VoidDelegate();

        private readonly log4net.ILog _log;
        private readonly RazerManager _manager;
        private Friend _friend;

        public ChatWindow(RazerManager manager, CSteamID friendId)
        {
            if (Program.SteamFriends == null)
                throw new Exception("ChatWindow does not work without steam features enabled");

            _log = LogManager.GetLogger(this);
            _log.DebugFormat(">> ChatWindow({0})", friendId.Render());

            InitializeComponent();

            _manager = manager;

            _log.Debug("Setting friend variable");
            _friend = Program.SteamFriends.GetFriendBySteamId(friendId);
            ChatTitle.Text = "Chatting with " + _friend.GetName();
            _log.Debug("Subscribing to ChatMessageReceived event on friend object");
            _friend.ChatMessageReceived += HandleChatMessage;
            _log.Debug("Adding previous messages to chat history");
            foreach (var message in _friend.ChatHistory)
            {
                WriteChatMessage(message.Sender, message.Message);
            }
            _log.Debug("Subscribing to SteamFriends.FriendsUpdated event");
            Program.SteamFriends.FriendsUpdated += HandleFriendsUpdated;

            _log.Debug("<< ChatWindow()");
        }

        private void SendMessage()
        {
            _log.Debug(">> SendMessage()");
            var message = EntryBox.Text;
            if (string.IsNullOrEmpty(message))
            {
                _log.Debug("Message empty, aborting");
                _log.Debug("<< SendMessage()");
                return;
            }
            _friend.SendMessage(message + "\0");
            EntryBox.Clear();
            EntryBox.Invalidate();
            //ChatTitle.Focus();
            _log.Debug("<< SendMessage()");
        }

        private void ChatHistoryTextChanged(object sender, EventArgs e)
        {
            ChatHistory.Invalidate(true);
        }

        private void EntryBoxKeyDown(object sender, KeyEventArgs e)
        {
            //_log.Debug(">> EntryBoxKeyDown([sender], [e])");
            if (e.KeyCode != Keys.Enter || string.IsNullOrEmpty(EntryBox.Text))
            {
                EntryBox.Invalidate();
                //_log.Debug("<< EntryBoxKeyDown()");
                return;
            }

            _log.Debug("Enter key pressed, sending message");

            SendMessage();
            //_log.Debug("<< EntryBoxKeyDown()");
        }

        private void EntryBoxTextChanged(object sender, EventArgs e)
        {
            //_log.Debug(">> EntryBoxTextChanged([sender], [e])");
            EntryBox.Text = EntryBox.Text.TrimStart('\n', '\r', ' ');
            EntryBox.Invalidate();
            //_log.Debug("<< EntryBoxTextChanged()");
        }

        private void EntryBoxLeave(object sender, EventArgs e)
        {
            _log.Debug(">> EntryBoxLeave()");
            SendMessage();
            _log.Debug("<< EntryBoxLeave()");
        }

        private void WriteChatMessage(CSteamID sender, string message)
        {
            _log.DebugFormat(">> WriteChatMessage({0}, \"{1}\")", sender.Render(), message);
            bool isMe = Program.SteamClient.IsMe(sender);

            string name = isMe
                              ? Program.SteamClient.GetMyName()
                              : Program.SteamFriends.GetFriendBySteamId(sender).GetName();
            ChatHistory.AppendText("");
            ChatHistory.SelectionColor = Color.CornflowerBlue;
            ChatHistory.AppendText(name + ": ");
            ChatHistory.SelectionColor = isMe ? Color.DarkGray : Color.White;
            ChatHistory.AppendText(message + Environment.NewLine);
            ChatHistory.ScrollToCaret();
            ChatHistory.Invalidate();
            _log.Debug("<< WriteChatMessage()");
        }

        private void HandleChatMessage(object sender, ChatMessageEventArgs e)
        {
            _log.Debug(">> HandleChatMessage([sender], [e])");
            try
            {
                var msg = e.Message;
                var message = msg.Message;
                if (ChatHistory.InvokeRequired)
                    ChatHistory.Invoke((VoidDelegate) (() => WriteChatMessage(msg.Sender, message)));
                else
                    WriteChatMessage(msg.Sender, message);
            }
            catch (ObjectDisposedException ex)
            {
                _log.WarnFormat("ObjectDisposedException while attempting to add message to chatlog. Message was: {0}", ex.Message);
            }
            finally
            {
                _log.Debug("<< HandleChatMessage()");
            }
        }

        private void HandleFriendsUpdated(object sender, EventArgs e)
        {
            _log.Debug(">> HandleFriendsUpdated([sender], [e])");
            _log.Debug("Unsubscribing from ChatMessageReceived event on friend object");
            _friend.ChatMessageReceived -= HandleChatMessage;
            var newFriend = Program.SteamFriends.GetFriendBySteamId(_friend.SteamID);
            if (newFriend == null || !newFriend.Online)
            {
                _log.Info("Friend null or offline, closing chat window");
                _friend = null;
                Program.QueueForm(new FriendsWindow(_manager));
                Close();
                return;
            }
            _friend = newFriend;
            _log.Debug("Subscribing to ChatMessageReceived on friend object");
            _friend.ChatMessageReceived += HandleChatMessage;
            _log.Debug("<< HandleFriendsUpdated()");
        }

        private void ChatWindowFormClosing(object sender, FormClosingEventArgs e)
        {
            _log.Debug(">> ChatWindowFormClosing([sender], [e])");
            _log.Debug("Unsubscribing from SteamFriends.FriendsUpdated event");
            Program.SteamFriends.FriendsUpdated -= HandleFriendsUpdated;
            _log.Debug("Unsubscribing from ChatMessageReceived event on friend object");
            _friend.ChatMessageReceived -= HandleChatMessage;
            _log.Debug("<< ChatWindowFormClosing()");
        }

        public IEnumerable<Control> GetKeyboardEnabledControls()
        {
            return new List<Control> {EntryBox};
        }

        public void HandleGesture(object sender, GestureEventArgs e)
        {
            if (e.GestureType != RazerAPI.GestureType.Press || e.Parameter != 0)
                return;

            if (e.Y >= EntryBox.Location.Y)
                EntryBox.Focus();
            else
                ChatTitle.Focus(); // Defocus the entry box
        }
    }
}
