﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MyItems.ViewModels
{
    public class MainPageViewModel: BaseViewModel
    {
        private ObservableCollection<Task> _tasks;
        public ICommand ListCommand { private set; get; }

        public ObservableCollection<Task> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged(nameof(Tasks));
            }
        }

        public MainPageViewModel() 
        {
            try
            {
                ViewName = "Διαχείριση Αρχικής Σελίδας";
                //SelectedIndex = 0;
                ListCommand = new Command(GetService); //,CanGetService
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void GetService(object obj)
        {
            try
            {
                //Shops = service.GetCustomers();
                ((Command)ListCommand).ChangeCanExecute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //private bool CanGetService(object arg)
        //{
        //    try
        //    {
        //        //return Shops == null;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        return false;
        //    }
        //}
    }
}
