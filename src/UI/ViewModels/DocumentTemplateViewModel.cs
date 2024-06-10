using Microsoft.Win32;
using NextGen.src.Services;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Views.UserControls;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace NextGen.src.UI.ViewModels
{
    public class DocumentTemplateViewModel : BaseViewModel
    {
        private readonly TemplateService _templateService;
        private string _selectedFilePath;
        private string _templateName;

        public DocumentTemplateViewModel()
        {
            _templateService = new TemplateService();
            SelectFileCommand = new RelayCommand(SelectFile);
            UploadTemplateCommand = new RelayCommand(UploadTemplate, CanUploadTemplate);
        }

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged(nameof(SelectedFilePath));
                ((RelayCommand)UploadTemplateCommand).RaiseCanExecuteChanged();
            }
        }

        public string TemplateName
        {
            get => _templateName;
            set
            {
                _templateName = value;
                OnPropertyChanged(nameof(TemplateName));
                ((RelayCommand)UploadTemplateCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand SelectFileCommand { get; }
        public ICommand UploadTemplateCommand { get; }

        private void SelectFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
            }
        }

        private async void UploadTemplate()
        {
            try
            {
                byte[] fileContent = File.ReadAllBytes(SelectedFilePath);
                _templateService.SaveTemplate(TemplateName, fileContent);
                await ShowCustomMessageBox("Шаблон успешно загружен.", "Успех", CustomMessageBox.MessageKind.Success);
            }
            catch (Exception ex)
            {
                await ShowCustomMessageBox($"Произошла ошибка при загрузке шаблона: {ex.Message}", "Ошибка", CustomMessageBox.MessageKind.Error);
            }
        }

        private bool CanUploadTemplate()
        {
            return !string.IsNullOrEmpty(SelectedFilePath) && !string.IsNullOrEmpty(TemplateName);
        }

        private async Task<bool> ShowCustomMessageBox(string message, string title = "Уведомление", CustomMessageBox.MessageKind kind = CustomMessageBox.MessageKind.Notification, bool showSecondaryButton = false, string primaryButtonText = "ОК", string secondaryButtonText = "Отмена")
        {
            var view = new CustomMessageBox(message, title, kind, showSecondaryButton, primaryButtonText, secondaryButtonText);
            var result = await DialogHost.Show(view, "RootDialogHost");
            return result is bool boolean && boolean;
        }
    }
}
