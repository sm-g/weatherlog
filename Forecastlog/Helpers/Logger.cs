using System;

namespace Weatherlog.ViewModels
{
    public class Logger
    {
        readonly object _allTextLock = new Object();
        readonly object _lastLineLock = new Object();
        bool _lastLineEnabled = false;
        bool _newLineEnabled = true;

        string _text = "";
        string _lastLine = "";

        public string AllText
        {
            get
            {
                lock (_allTextLock)
                {
                    return _text;
                }
            }
        }
        public string LastLine
        {
            get
            {
                lock (_lastLineLock)
                {
                    return _lastLine;
                }
            }
        }

        public void AddLine(string logMessage)
        {
            lock (_allTextLock)
            {
                if (_newLineEnabled)
                    _text += logMessage + Environment.NewLine;
                else
                    _text += logMessage;
            }
            lock (_lastLineLock)
            {
                if (_lastLineEnabled)
                {
                    _lastLine = logMessage;
                }
            }
        }

        public Logger(bool saveLastLine = false, bool addNewLineSign = true)
        {
            _lastLineEnabled = saveLastLine;
            _newLineEnabled = addNewLineSign;
        }
    }
}
