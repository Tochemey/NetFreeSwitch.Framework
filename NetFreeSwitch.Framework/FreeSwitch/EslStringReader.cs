/*
Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
*/

using System;
using System.Collections.Generic;

namespace NetFreeSwitch.Framework.FreeSwitch {
    public class EslStringReader {
        private readonly Stack<int> _savedPositions = new Stack<int>();
        private int _pos;
        private string _text;

        public EslStringReader(string text)
        {
            _pos = 0;
            _text = text;
        }

        public bool EOF
        {
            get { return IsEOF(_pos); }
        }

        /// <summary>
        ///     true if we are currently on a emty line
        /// </summary>
        public bool EOL
        {
            get
            {
                if (EOF)
                    return false;
                if (_text[_pos] == '\n')
                    return true;
                if (_pos < _text.Length - 2)
                    return _text[_pos] == '\r' && _text[_pos + 1] == '\n';
                return false;
            }
        }

        public void SavePos()
        {
            _savedPositions.Push(_pos);
        }

        public void RestorePos()
        {
            if (_savedPositions.Count > 0)
                _pos = _savedPositions.Pop();
        }

        public void ClearSavedPositions()
        {
            _savedPositions.Clear();
        }

        private bool IsEOF(int pos)
        {
            return pos >= _text.Length;
        }

        public bool IsEmptyLine(bool skipWhitespaces)
        {
            if (EOF)
                return false;
            if (_pos < _text.Length - 1)
                return _text[_pos] == '\n';
            if (_pos < _text.Length - 2)
                return _text[_pos] == '\r' && _text[_pos + 1] == '\n';

            return false;
        }

        public string PeekWord(bool skipWs, bool stopAtEOL)
        {
            int startpos = _pos;
            if (skipWs)
                SkipWs();
            int pos = FindWs(stopAtEOL);
            _pos = startpos;

            return pos != -1 ? _text.Substring(_pos, pos - _pos) : string.Empty;
        }

        public string GetWord(bool skipWs, bool stopAtEOL)
        {
            int startpos = _pos;
            if (skipWs)
                SkipWs();
            int pos = FindWs(stopAtEOL);

            _pos = pos;

            return pos != -1 ? _text.Substring(startpos, pos - startpos) : string.Empty;
        }

        public string ReadLine()
        {
            return ReadLine(false);
        }

        public string ReadLine(bool trimWhiteSpaces)
        {
            if (EOF)
                return string.Empty;

            if (trimWhiteSpaces)
                SkipWs();

            // loop until we get new line chars
            int startpos = _pos;
            while (!EOF && !IsEOL())
                ++_pos;

            // Save _pos, trim whitespaces, restore _pos
            int pos = _pos;
            if (trimWhiteSpaces)
                TrimEnd();
            int endpos = _pos;
            _pos = pos;

            // Move to after new line chars   
            if (!EOF && _text[_pos] == '\r')
                ++_pos;
            if (!EOF && _text[_pos] == '\n')
                ++_pos;

            // .. and the result is line without whitespaces at the end and without new line chars.
            return _text.Substring(startpos, endpos - startpos);
        }

        public string ReadToEmptyLine()
        {
            int chars = 4;
            int pos = _text.IndexOf("\n\n", _pos, StringComparison.Ordinal);
            if (pos == -1)
            {
                chars = 2;
                pos = _text.IndexOf("\n\n", _pos, StringComparison.Ordinal);
            }
            if (pos == -1)
                return string.Empty;

            int startpos = _pos;
            _pos = pos + chars;
            return _text.Substring(startpos, pos - startpos);
        }

        public string Read(int chars)
        {
            if (_text.Length - _pos < chars)
                return string.Empty;

            string result = _text.Substring(_pos, chars);
            _pos += chars;
            return result;
        }

        public string ReadToEnd()
        {
            string result = _text.Substring(_pos, _text.Length - _pos);
            _pos = _text.Length;
            return result;
        }

        public string Read(char delimiter, bool skipWs)
        {
            if (skipWs)
                SkipWs();

            // find delimiter
            int startpos = _pos;
            while (!EOF && _text[_pos] != delimiter)
                ++_pos;

            // did not find it, reset pos
            if (EOF)
            {
                _pos = startpos;
                return string.Empty;
            }
            ++_pos; // skip delimiter
            int endpos = TrimEnd(); // skip 
            return _text.Substring(startpos, endpos - startpos - 1);
        }

        private int TrimEnd()
        {
            if (EOF || !IsWS(_text[_pos]))
                return _pos;

            int pos = _pos;
            while (IsWS(_text[pos]))
                --pos;
            return pos + 1;
        }

        private int FindWs(bool stopAtEOL)
        {
            if (!stopAtEOL)
                return FindWs();

            int pos = _pos;
            while (!IsEOF(pos) && !IsEOL(_text[pos]) && !IsWS(_text[pos]))
                ++pos;

            return IsEOF(pos) ? -1 : pos;
        }

        private bool IsEOL()
        {
            if (EOF)
                return true;
            if (_text[_pos] == '\r')
                return true;
            if (_text[_pos] == '\n')
                return true;
            return false;
        }

        private bool IsEOL(char ch)
        {
            return ch == '\r' || ch == '\n';
        }

        private bool IsWS(char ch)
        {
            return ch == '\t' || ch == ' ';
        }

        private int FindWs()
        {
            int pos = _pos;
            while (!IsEOF(pos) && !IsWS(_text[pos]))
                ++pos;

            return IsEOF(pos) ? -1 : pos;
        }

        public void SkipWs()
        {
            while (!EOF && IsWS(_text[_pos]))
                ++_pos;
        }

        public void RemoveUsedContent()
        {
            _text = _text.Remove(0, _pos);
            _pos = 0;
        }

        public void Append(string value)
        {
            _text += value;
        }

        public void Clear()
        {
            _text = string.Empty;
            _pos = 0;
        }
    }
}
