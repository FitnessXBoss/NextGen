using Microsoft.Win32;
using NextGen.src.Services;
using NextGen.src.UI.Helpers;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

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

        private void UploadTemplate()
        {
            try
            {
                byte[] fileContent = File.ReadAllBytes(SelectedFilePath);
                _templateService.SaveTemplate(TemplateName, fileContent);
                MessageBox.Show("Шаблон успешно загружен.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке шаблона: {ex.Message}");
            }
        }

        private bool CanUploadTemplate()
        {
            return !string.IsNullOrEmpty(SelectedFilePath) && !string.IsNullOrEmpty(TemplateName);
        }
    }
}
