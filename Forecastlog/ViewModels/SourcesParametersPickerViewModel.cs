using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Weatherlog.Models.Parameters;
using Weatherlog.Models.Sources;

namespace Weatherlog.ViewModels
{
    public class SourcesParametersPickerViewModel : BaseViewModel
    {
        ICommand _selectAllSourcesListCommand;
        ICommand _selectAllParametersListCommand;
        ObservableCollection<IAbstractDataSource> _checkedSources = new ObservableCollection<IAbstractDataSource>();
        ObservableCollection<AbstractParameter> _checkedParameters = new ObservableCollection<AbstractParameter>();
        bool _allSourcesChecked;
        bool _allParametersChecked;
        bool _sourceListManualAdding;

        public override string Name { get { return ""; } }

        public ObservableCollection<AbstractParameter> Parameters { get; private set; }
        public ObservableCollection<AbstractParameter> CheckedParameters { get { return _checkedParameters; } }

        public ObservableCollection<IAbstractDataSource> Sources { get; private set; }
        public ObservableCollection<IAbstractDataSource> CheckedSources { get { return _checkedSources; } }

        public bool IsAllSourcesChecked
        {
            get
            {
                return _allSourcesChecked;
            }
            set
            {
                if (_allSourcesChecked != value)
                {
                    _allSourcesChecked = value;
                    OnPropertyChanged(() => IsAllSourcesChecked);
                }
            }
        }
        public bool IsAllParametersChecked
        {
            get
            {
                return _allParametersChecked;
            }
            set
            {
                if (_allParametersChecked != value)
                {
                    _allParametersChecked = value;
                    OnPropertyChanged(() => IsAllParametersChecked);
                }
            }
        }
        public ICommand SelectAllSourcesListCommand
        {
            get
            {
                if (_selectAllSourcesListCommand == null)
                {
                    _selectAllSourcesListCommand = new RelayCommand(SelectAllSourcesInList);
                }

                return _selectAllSourcesListCommand;
            }
        }

        public ICommand SelectAllParametersListCommand
        {
            get
            {
                if (_selectAllParametersListCommand == null)
                {
                    _selectAllParametersListCommand = new RelayCommand(SelectAllParametersInList);
                }

                return _selectAllParametersListCommand;
            }
        }

        public SourcesParametersPickerViewModel()
        {
            Sources = new ObservableCollection<IAbstractDataSource>(SourcesDirector.Instance.AllDataSources);
            Parameters = new ObservableCollection<AbstractParameter>(
                ParametersFactory.KnownParameterTypesNames.Select(type => ParametersFactory.CreateParameter(type, 1)));
            CheckedSources.CollectionChanged += (s, e) =>
            {
                if (!_sourceListManualAdding)
                {
                    if (CheckedSources.Count == Sources.Count)
                        IsAllSourcesChecked = true; // consider CheckedSources may contain only elements of Sources
                    else
                        IsAllSourcesChecked = false;
                }
            };
            CheckedParameters.CollectionChanged += (s, e) =>
            {
                if (!_sourceListManualAdding)
                {
                    if (CheckedParameters.Count == Parameters.Count)
                        IsAllParametersChecked = true; // consider CheckedParameters may contain only elements of Parameters
                    else
                        IsAllParametersChecked = false;
                }
            };
            IsAllSourcesChecked = false;
            IsAllParametersChecked = false;
        }

        private void SelectAllSourcesInList()
        {
            _sourceListManualAdding = true;
            if (IsAllSourcesChecked)
            {
                CheckedSources.Clear();
                foreach (var source in Sources)
                {
                    CheckedSources.Add(source);
                }
            }
            else
            {
                CheckedSources.Clear();
            }
            _sourceListManualAdding = false;
        }
        private void SelectAllParametersInList()
        {
            _sourceListManualAdding = true;
            if (IsAllParametersChecked)
            {
                CheckedParameters.Clear();
                foreach (var parameter in Parameters)
                {
                    CheckedParameters.Add(parameter);
                }
            }
            else
            {
                CheckedParameters.Clear();
            }
            _sourceListManualAdding = false;
        }

    }
}
