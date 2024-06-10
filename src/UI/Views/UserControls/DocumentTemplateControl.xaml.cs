﻿using NextGen.src.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NextGen.src.UI.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для DocumentTemplateControl.xaml
    /// </summary>
    public partial class DocumentTemplateControl : UserControl
    {
        public DocumentTemplateControl()
        {
            InitializeComponent();
            DataContext = new DocumentTemplateViewModel();

        }
    }
}
