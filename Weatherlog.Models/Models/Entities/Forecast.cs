using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Weatherlog.Models.Parameters;

namespace Weatherlog.Models
{
    public class Forecast
    {
        private List<AbstractParameter> _parameters;
        private DateTime _creationTime;
        private DateTime _validTime;

        /// <summary>
        /// Local time point, which is described by the forecast.
        /// </summary>
        [JsonProperty(PropertyName = "valid")]
        public DateTime ValidTime
        {
            get { return _validTime; }
        }

        /// <summary>
        /// Local time when the forecast was made.
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public DateTime CreationTime
        {
            get { return _creationTime; }
        }

        [JsonProperty(PropertyName = "parameters")]
        public IEnumerable<AbstractParameter> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="creationTime">Local time when the forecast was made.</param>
        /// <param name="validTime">Local time point, which is described by the forecast.</param>
        public Forecast(List<AbstractParameter> parameters, DateTime creationTime, DateTime validTime)
        {
            _parameters = parameters.Where(p => p.IsValid).ToList();
            // _source = source;
            _creationTime = creationTime;
            _validTime = validTime;
        }

        public override string ToString()
        {
            return String.Format("v:{0} c:{1} p:{2}", ValidTime, CreationTime, Parameters.Count());
        }
    }
}
