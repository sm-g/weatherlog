using System;
using Weatherlog.Models;

namespace Weatherlog.ViewModels
{
    public class Settings
    {
        Station _currentStation;

        public event EventHandler CurrentStationChanged;

        public Station CurrentStation
        {
            get
            {
                return _currentStation;
            }
            set
            {
                if (_currentStation != value)
                {
                    _currentStation = value;
                    OnCurrentStationChanged();
                }
            }
        }

        protected virtual void OnCurrentStationChanged()
        {
            var handler = this.CurrentStationChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}
