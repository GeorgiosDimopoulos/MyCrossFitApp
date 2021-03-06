﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyItems.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AthensPage : TabbedPage
    {
        //public ObservableCollection<Task> myList;
        private List<Task> generalList;        
        private bool editOption;
        private IOrderedEnumerable<Task> _sortedList;
        //private int wholeCost;
        private Task currentTask;
        private Task mainTask;
        private DateTime selectedDate;

        public AthensPage ()
        {
            try
            {
                InitializeComponent();
                NavigationPage.SetHasNavigationBar(this, true);
                NavigationPage.SetHasBackButton(this, false);                
                //FillPickers();
                //FillingListViews();
                //CleaningEntries();
            }
            catch (Exception ex)
            {
                DisplayAlert("AthensPage", ex.Message, "OK");
            }
        }

        protected async void FillPickers()
        {
            try
            {
                EditGeneralCostPicker.Items.Clear();
                EditGeneralCostPicker.Items.Add("Αλλαγή Τιμής");
                EditGeneralCostPicker.Items.Add("Νέα Προθεσμία");
                AthensExodusChoicesPicker.Items.Clear();
                AthensToDoChoicesPicker.Items.Clear();
                AthensToDoChoicesPicker.Items.Add("Διαγραφή");
                AthensToDoChoicesPicker.Items.Add("Μετονομασία");
                AthensToDoChoicesPicker.Items.Add("Αλλαγή Ημερομηνίας");
                AthensCostChoicesPicker.Items.Clear();
                AthensCostChoicesPicker.Items.Add("Διαγραφή");
                AthensCostChoicesPicker.Items.Add("Μετονομασία");
                AthensCostChoicesPicker.Items.Add("Αλλαγή Τιμής");
                AthensExodusChoicesPicker.Items.Add("Διαγραφή");
                AthensExodusChoicesPicker.Items.Add("Μετονομασία");
                AthensExodusChoicesPicker.Items.Add("Αλλαγή Ημερομηνίας");
            }
            catch (Exception e)
            {                
                await DisplayAlert("FillPickers Error: ", e.Message, "OK");
            }            
        }

        protected async void FillingListViews()
        {
            try
            {                
                AthensToDoListView.ItemsSource = _sortedList.Where(x => x.Type.Equals(15));
                AthensExodusListView.ItemsSource = _sortedList.Where(x => x.Type.Equals(14));
                AthensCostsListView.ItemsSource = _sortedList.Where(x => x.Type.Equals(17));
            }
            catch (Exception e)
            {
                await DisplayAlert("FillingListViews Error: ", e.Message, "OK");
            }
        }

        protected void CleaningEntries()
        {
            try
            {
                AthensToDoEntry.Text = "";
                AthensExodusEntry.Text = "";
                AthensCostEntry.Text = "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);                
            }
        }

        protected override async void OnAppearing()
        {
            try
            {
                editOption = false;
                generalList = new List<Task>();
                generalList = await App.ItemController.GetTasks(); // myList = new ObservableCollection<Task>(generalList);
                _sortedList = from cTask in generalList orderby cTask.Date, cTask.Date select cTask;
                foreach (var t in generalList)
                {
                    if (t.Type != 22) continue;
                    mainTask = t;
                    LastDayCostLabel.Text = mainTask.Date.ToString("dd/MM", CultureInfo.InvariantCulture); ;
                    GeneralCostPriceLabel.Text = mainTask.Price + "€";
                }
                FillPickers();
                FillingListViews();
                CleaningEntries();
                CountGeneralCosts();
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception e)
            {
                await DisplayAlert("OnAppearing", e.Message, "OK");
            }
        }
        
        private bool CheckDuplicates(string possibleText, int taskType)
        {
            try
            {
                foreach (var item in _sortedList.Where(x=>x.Type == taskType)) // myList
                {
                    if (possibleText == item.Text)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                DisplayAlert("CheckDuplicates", e.Message, "OK");
                return false;
            }
        }

        private void CountGeneralCosts()
        {
            try
            {
                double allCosts = 0.0;
                foreach (var expense in _sortedList)
                {
                    if (expense.Type == 17)
                    {
                        allCosts += double.Parse(expense.Price); // eg 700
                    }
                };
                var stringCosts = allCosts.ToString(CultureInfo.InvariantCulture); // "700"
                var stringCostsFinal = stringCosts.Remove(stringCosts.Length - 1); // "70"
                double almostFinalDouble = double.Parse(stringCostsFinal); // 70                
                double mainTaskPrice = double.Parse(mainTask.Price.Remove(mainTask.Price.Length-1)); // eg 40
                almostFinalDouble += mainTaskPrice;// 70 + 40 = 110
                AllCostsLabel.Text = "Σύνολo: " + almostFinalDouble + " €";
                //wholeCost = Convert.ToInt32(almostFinalDouble);
            }
            catch (Exception e)
            {
                DisplayAlert("CountGeneralCosts", e.Message, "OK");
            }
        }

        private async void AthensToDoChoicesPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (AthensToDoChoicesPicker.SelectedIndex == 0) // delete task
                {
                    UserDialogs.Instance.ShowLoading();
                    //myList.Remove(currentTask);
                    generalList.Remove(currentTask);
                    await App.ItemController.DeleteTask(currentTask);
                    AthensToDoListView.ItemsSource = null;
                    //AthensToDoListView.ItemsSource = myList.ToList().Where(x => x.Type.Equals(15));
                    _sortedList = from cTask in generalList orderby cTask.Date, cTask.Date select cTask;
                    AthensToDoListView.ItemsSource = _sortedList.Where(x => x.Type.Equals(15));
                    UserDialogs.Instance.HideLoading();
                    await DisplayAlert(null, "Επιτυχής Διαγραφή!", "OK");
                }
                else if (AthensToDoChoicesPicker.SelectedIndex == 1) // rename task
                {
                    var result = await UserDialogs.Instance.PromptAsync("Μετονομασία", null, "Μετονομασία Υποχρέωσης", "Ακυρο", currentTask.Text, inputType: InputType.Default);
                    if (string.IsNullOrEmpty(result.Text))
                    {
                        await DisplayAlert(null, "Πληκτρολόγησε κάτι!", "OK");
                        return;
                    }
                    UserDialogs.Instance.ShowLoading();
                    currentTask.Text = result.Text;
                    await App.ItemController.UpdateTask(currentTask);
                    AthensToDoListView.ItemsSource = null;
                    AthensToDoListView.ItemsSource = _sortedList.ToList().Where(x => x.Type.Equals(15));
                    UserDialogs.Instance.HideLoading();
                    await DisplayAlert(null, "Επιτυχής Μετονομασία Υποχρέωσης!", "OK");
                }
                else if (AthensToDoChoicesPicker.SelectedIndex == 2)
                {
                    ToDoDatePicker.IsVisible = true;
                    ToDoDatePicker.Focus();
                    editOption = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("AthensToDoChoicesPicker_SelectedIndexChanged error", ex.Message, "OK");
            }
        }

        private void AthensToDoChoicesPicker_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                AthensToDoChoicesPicker.IsVisible = false;
                AthensToDoChoicesPicker.Unfocus();
                AthensToDoListView.SelectedItem = null;
                AthensToDoChoicesPicker.SelectedItem = null;                
            }
            catch (Exception ex)
            {
                DisplayAlert("AthensToDoChoicesPicker_SelectedIndexChanged error", ex.Message, "OK");
            }
        }

        private async void ToDoImageButton_Clicked(object sender, EventArgs e)
        {
            try
            {                
                if (string.IsNullOrEmpty(AthensToDoEntry.Text))
                {
                    await DisplayAlert(null, "Γράψτε κάτι για προσθήκη!", "OK");
                    return;
                }
                if (!CheckDuplicates(AthensToDoEntry.Text, 15))
                {
                    await DisplayAlert("ΣΦΑΛΜΑ", "Υπάρχει ήδη, δοκιμάστε κάτι άλλο!", "ΟΚ");
                    AthensToDoEntry.Text = "";                    
                }
                //if (await DisplayAlert("Προσθήκη", "Προσθήκη Ημερομηνίας", "ΟΚ", "OXI"))
                editOption = false;
                ToDoDatePicker.IsVisible = true;
                ToDoDatePicker.Focus(); //Item_Added(AthensToDoEntry.Text, 15,DateTime.Today,"0");
                    //UserDialogs.Instance.ShowLoading();                   
                    //var task = new Task
                    //{
                    //    Text = AthensToDoEntry.Text, Type = 15, Date = DateTime.Today
                    //};
                    //await App.ItemController.InsertTask(task);
                    //AthensToDoListView.ItemsSource = null;
                    //_sortedList = from cTask in generalList orderby cTask.Date, cTask.Date select cTask;
                    //AthensToDoListView.ItemsSource = _sortedList.Where(x => x.Type.Equals(15));
                    //AthensToDoEntry.Text = "";
                    //AthensToDoListView.SelectedItem = null;
                    //UserDialogs.Instance.HideLoading();
                    //await DisplayAlert(null, "Επιτυχής Προσθήκη!", "OK");                                
            }
            catch (Exception ex)
            {
                await DisplayAlert("ImageButton_Clicked error", ex.Message, "OK");
            }
        }

        private void AthensToDoListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var task = (Task)AthensToDoListView.SelectedItem;
                currentTask = task;
                AthensToDoChoicesPicker.IsVisible = true;
                AthensToDoChoicesPicker.Focus();
            }
            catch (Exception ex)
            {
                DisplayAlert("AthensToDoListView_ItemTapped error", ex.Message, "OK");
            }
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            try
            {
                Environment.Exit(0);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void AthensExodusListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var task = (Task)AthensExodusListView.SelectedItem;
                currentTask = task;
                AthensExodusChoicesPicker.IsVisible = true;
                AthensExodusChoicesPicker.Focus();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void AthensExodusChoicesPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (AthensExodusChoicesPicker.SelectedIndex == 0) // delete task
                {
                    UserDialogs.Instance.ShowLoading();
                    generalList.Remove(currentTask);
                    await App.ItemController.DeleteTask(currentTask);
                    AthensExodusListView.ItemsSource = null;
                    AthensExodusListView.ItemsSource = _sortedList.ToList().Where(x => x.Type.Equals(14));
                    UserDialogs.Instance.HideLoading();
                    await DisplayAlert(null, "Επιτυχής Διαγραφή!", "OK");
                }
                else if (AthensExodusChoicesPicker.SelectedIndex == 1) // rename task
                {
                    var result = await UserDialogs.Instance.PromptAsync("Μετονομασία", null, "Μετονομασία Εξόδου", "Ακυρο", currentTask.Text, inputType: InputType.Default);
                    if (string.IsNullOrEmpty(result.Text))
                    {
                        await DisplayAlert(null, "Πληκτρολόγησε κάτι!", "OK");
                        return;
                    }
                    UserDialogs.Instance.ShowLoading();
                    currentTask.Text = result.Text;
                    await App.ItemController.UpdateTask(currentTask);
                    AthensExodusListView.ItemsSource = null;
                    AthensExodusListView.ItemsSource = _sortedList.ToList().Where(x => x.Type.Equals(14));
                    UserDialogs.Instance.HideLoading();
                    await DisplayAlert(null, "Επιτυχής Μετονομασία!", "OK");
                }
                else if (AthensExodusChoicesPicker.SelectedIndex == 2) // date of task
                {
                    ExodusDatePicker.IsVisible = true;
                    ExodusDatePicker.Focus();
                    editOption = true;
                    //ExodusDatePicker.DateSelected += EditExodusDatePicker_OnDateSelected;
                }
                AthensExodusChoicesPicker.IsVisible = false;
            }
            catch (Exception exception)
            {
                await DisplayAlert(null, exception.Message, "OK");
            }
        }

        private async void AthensExodusChoicesPicker_OnUnfocused(object sender, FocusEventArgs e)
        {
            try
            {
                AthensExodusChoicesPicker.IsVisible = false;
                AthensExodusChoicesPicker.Unfocus();
                AthensExodusChoicesPicker.SelectedItem = null;
                AthensExodusListView.SelectedItem = null;
            }
            catch (Exception exception)
            {
                await DisplayAlert("AthensExodusChoicesPicker_OnUnfocused", exception.Message, "OK");
            }
        }

        private async void ExodusButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(AthensExodusEntry.Text))
                {
                    await DisplayAlert(null, "Γράψτε κάτι για προσθήκη!", "OK");
                    return;
                }
                if (!CheckDuplicates(AthensToDoEntry.Text, 14))
                {
                    await DisplayAlert("ΣΦΑΛΜΑ", "Υπάρχει ήδη, δοκιμάστε κάτι άλλο!", "ΟΚ");
                    AthensToDoEntry.Text = "";
                    return;
                }
                ExodusDatePicker.IsVisible = true;
                ExodusDatePicker.Focus();
            }
            catch (Exception exception)
            {
                await DisplayAlert("ExodusButton_OnClicked", exception.Message, "OK");
            }
        }

        private async void ToDoDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            try
            {                
                if (editOption)
                {
                    UserDialogs.Instance.ShowLoading();
                    currentTask.Date= ToDoDatePicker.Date;
                    if (currentTask.Date < DateTime.Today)
                    {
                        await DisplayAlert("Σφάλμα", "Επιλέξτε μία μελλοντική μέρα!", "OK");
                        return;
                    }
                    await App.ItemController.UpdateTask(currentTask);
                    AthensToDoListView.ItemsSource = null;
                    AthensToDoListView.ItemsSource = _sortedList.ToList().Where(x => x.Type.Equals(15));
                    AthensToDoListView.SelectedItem = null;                    
                    UserDialogs.Instance.HideLoading();
                    await DisplayAlert(null, "Επιτυχής Αλλαγη Ημερομηνίας!", "OK");
                }
                else
                {
                    Item_Added(AthensToDoEntry.Text, 15, ToDoDatePicker.Date, "0");
                    //UserDialogs.Instance.ShowLoading();
                    //selectedDate = ToDoDatePicker.Date;
                    //var task = new Task
                    //{
                    //    Text = AthensToDoEntry.Text,
                    //    Date = selectedDate.Date,
                    //    Type = 15
                    //};
                    //generalList.Add(task);
                    //await App.ItemController.InsertTask(task);
                    //AthensToDoListView.ItemsSource = null;
                    //AthensToDoListView.ItemsSource = _sortedList.ToList().Where(x => x.Type.Equals(15));
                    //UserDialogs.Instance.HideLoading();
                    //AthensToDoListView.SelectedItem = null;
                    //await DisplayAlert("Προσθήκη", "Νέα υποχρέωση προστέθηκε", "OK");                    
                }
                ToDoDatePicker.Unfocus();
                ToDoDatePicker.IsVisible = false;
                //ToDoDatePicker.Date = DateTime.Today.AddDays(-1);
            }
            catch (Exception exception)
            {
                await DisplayAlert(null, exception.Message, "OK");
            }
        }

        private async void AthensCostChoicesPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {        
                if (AthensCostChoicesPicker.SelectedIndex == 0) // delete
                {
                    UserDialogs.Instance.ShowLoading();                    
                    generalList.Remove(currentTask);
                    await App.ItemController.DeleteTask(currentTask);
                    CountGeneralCosts();
                    AthensCostsListView.ItemsSource = null;
                    AthensCostsListView.ItemsSource = _sortedList.ToList().Where(x => x.Type.Equals(17));
                    UserDialogs.Instance.HideLoading();
                    await DisplayAlert(null, "Επιτυχής Διαγραφή!", "OK");
                }
                else if(AthensCostChoicesPicker.SelectedIndex == 1) // rename
                {
                    var result = await UserDialogs.Instance.PromptAsync("Μετονομασία", null, "Μετονομασία Εξόδου", "Ακυρο", currentTask.Text, inputType: InputType.Default);
                    if (string.IsNullOrEmpty(result.Text))
                    {
                        await DisplayAlert(null, "Πληκτρολόγησε κάτι!", "OK");
                        return;
                    }
                    UserDialogs.Instance.ShowLoading();
                    currentTask.Text = result.Text;
                    await App.ItemController.UpdateTask(currentTask);
                    AthensCostsListView.ItemsSource = null;
                    AthensCostsListView.ItemsSource = _sortedList.ToList().Where(x => x.Type.Equals(17));
                    UserDialogs.Instance.HideLoading();
                    await DisplayAlert(null, "Επιτυχής Μετονομασία!", "OK");
                }
                else if (AthensCostChoicesPicker.SelectedIndex == 2) // edit price
                {
                    var result = await UserDialogs.Instance.PromptAsync("Τιμή", null, "Τιμή Εξόδου", "Ακυρο", currentTask.Price, inputType: InputType.Number);
                    if (string.IsNullOrEmpty(result.Text))
                    {
                        await DisplayAlert(null, "Πληκτρολόγησε κάτι!", "OK");
                        return;
                    }
                    UserDialogs.Instance.ShowLoading();
                    currentTask.Price = result.Text + ".0";
                    await App.ItemController.UpdateTask(currentTask);
                    CountGeneralCosts();
                    AthensCostsListView.ItemsSource = null;
                    AthensCostsListView.ItemsSource = _sortedList.ToList().Where(x => x.Type.Equals(17));
                    UserDialogs.Instance.HideLoading();
                    await DisplayAlert(null, "Επιτυχής Αλλαγή Τιμής!", "OK");
                }
                AthensCostChoicesPicker.IsVisible = false;
                AthensCostChoicesPicker.Unfocus();
                AthensCostsListView.SelectedItem = null;                
            }
            catch (Exception exception)
            {
                await DisplayAlert("AthensCostChoicesPicker_OnSelectedIndexChanged", exception.Message, "OK");
            }
        }

        private async void AthensCostChoicesPicker_OnUnfocused(object sender, FocusEventArgs e)
        {
            try
            {
                AthensCostChoicesPicker.Unfocus();
                AthensCostChoicesPicker.IsVisible = false;
                AthensCostChoicesPicker.SelectedItem = null;
            }
            catch (Exception exception)
            {
                await DisplayAlert("AthensCostChoicesPicker_OnUnfocused", exception.Message, "OK");
            }
        }

        private async void AthensCostsListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var task = (Task)AthensCostsListView.SelectedItem;
                currentTask = task;
                AthensCostChoicesPicker.IsVisible = true;
                AthensCostChoicesPicker.Focus();
                AthensCostsListView.SelectedItem = null;
            }
            catch (Exception exception)
            {
                await DisplayAlert("AthensCostsListView_OnItemTapped", exception.Message, "OK");
            }
        }

        private async void ExodusDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading();
                selectedDate = ExodusDatePicker.Date;
                if (selectedDate < DateTime.Today)
                {
                    await DisplayAlert("Σφάλμα", "Επιλέξτε μία μελλοντική μέρα!", "OK");
                    return;
                }
                if (editOption)
                {
                    UserDialogs.Instance.ShowLoading();
                    currentTask.Date = selectedDate;
                    await App.ItemController.UpdateTask(currentTask);
                    AthensExodusListView.ItemsSource = null;
                    AthensExodusListView.ItemsSource = _sortedList.Where(x => x.Type.Equals(14)); //generalList.ToList()
                    AthensExodusListView.SelectedItem = null;
                    UserDialogs.Instance.HideLoading();
                    await DisplayAlert("ΑΛΛΑΓΗ", "Επιτυχής αλλαγή ημερομηνίας", "OK");
                }
                else
                {
                    var task = new Task
                    {
                        Text = AthensExodusEntry.Text,
                        Date = selectedDate.Date,
                        Type = 14
                    };
                    generalList.Add(task);
                    await App.ItemController.InsertTask(task);
                    AthensExodusListView.ItemsSource = null;
                    AthensExodusListView.ItemsSource = _sortedList.Where(x => x.Type.Equals(14)); //generalList.ToList()
                    AthensExodusEntry.Text = "";
                    AthensExodusListView.SelectedItem = null;
                    UserDialogs.Instance.HideLoading();
                    //await DisplayAlert("Νέα Έξοδο", "Επιτυχής Προσθήκη Έξοδου","OK");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);                
            }            
        }
        
        protected override bool OnBackButtonPressed()
        {
            try
            {
                Environment.Exit(0);                
                return true;
            }
            catch (Exception e)
            {
                DisplayAlert("OnBackButtonPressed", e.Message, "OK");
                return false;
            }
        }

        private void ExodusDatePicker_OnUnfocused(object sender, FocusEventArgs e)
        {
            ExodusDatePicker.Unfocus();
            ExodusDatePicker.IsVisible = false;
            //ExodusDatePicker.Date = DateTime.Today.AddDays(-1);
        }

        private async void Item_Added(string inputText,int type, DateTime date,string inputPrice)
        {
            try
            {
                UserDialogs.Instance.ShowLoading();
                var task = new Task
                {
                    Text = inputText,
                    Type = type,
                    Date = date,
                    Price = inputPrice
                };
                generalList.Add(task);
                await App.ItemController.InsertTask(task);
                FillingListViews();
                CleaningEntries();
                currentTask = null;
                UserDialogs.Instance.HideLoading();
                await DisplayAlert("Προσθήκη", "Επιτυχώς προστέθηκε", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Button_OnClicked", ex.Message, "OK");
            }
        }

        private async void CostButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(AthensCostEntry.Text))
                {
                    await DisplayAlert(null, "Γράψτε κάτι για προσθήκη!", "OK");
                    return;
                }
                if (!CheckDuplicates(AthensToDoEntry.Text, 17))
                {
                    await DisplayAlert("ΣΦΑΛΜΑ", "Υπάρχει ήδη, δοκιμάστε κάτι άλλο!", "ΟΚ");
                    AthensToDoEntry.Text = "";
                    return;
                }
                var result = await UserDialogs.Instance.PromptAsync("Τιμή", null, "Προσθήκη", "Ακυρο", "Προσθήκη Τιμής", inputType: InputType.Number);
                if (string.IsNullOrEmpty(result.Text))
                {
                    await DisplayAlert(null, "Πληκτρολόγησε κάτι για τη τιμή!", "OK");
                    return;
                }
                UserDialogs.Instance.ShowLoading();
                var task = new Task
                {
                    Text = AthensCostEntry.Text,
                    Type = 17,
                    Price = result.Text
                };
                generalList.Add(task);
                await App.ItemController.InsertTask(task);
                AthensCostsListView.ItemsSource = null;
                AthensCostsListView.ItemsSource = _sortedList.ToList().Where(x => x.Type.Equals(17));
                AthensCostEntry.Text = "";
                //CountGeneralCosts();
                currentTask = null;
                UserDialogs.Instance.HideLoading();
                await DisplayAlert("Προσθήκη", "Νέο Έξοδο προστέθηκε", "OK");

            }
            catch (Exception exception)
            {
                await DisplayAlert("CostButton_OnClicked", exception.Message, "OK");
            }
        }
        
        private async void ToDoDatePicker_OnUnfocused(object sender, FocusEventArgs e)
        {
            try
            {
                ToDoDatePicker.IsVisible = false;
                ToDoDatePicker.Unfocus();
                //ToDoDatePicker.Date = DateTime.Today.AddDays(-1);
            }
            catch (Exception exception)
            {
                await DisplayAlert("ToDoDatePicker_OnUnfocused", exception.Message, "OK");
            }
        }

        private void EditGeneralCost_OnClicked(object sender, EventArgs e)
        {
            try
            {
                EditGeneralCostPicker.IsVisible = true;
                EditGeneralCostPicker.Focus();                
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void EditGeneralCostPicker_OnUnfocused(object sender, FocusEventArgs e)
        {
            try
            {
                EditGeneralCostPicker.IsVisible = false;
                EditGeneralCostPicker.Unfocus();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void EditGeneralCostPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                EditGeneralCostPicker.SelectedItem = null;
                if (EditGeneralCostPicker.SelectedIndex == 0) //edit price
                {
                    var result = await UserDialogs.Instance.PromptAsync("Τιμή γενικού εξόδου", null, "Αλλαγή", "Ακυρο", mainTask.Price, inputType: InputType.Number);
                    if (string.IsNullOrEmpty(result.Text))
                    {
                        await DisplayAlert(null, "Πληκτρολόγησε κάτι!", "OK");
                        return;
                    }
                    mainTask.Price = result.Text;
                    await App.ItemController.UpdateTask(mainTask);
                    GeneralCostPriceLabel.Text = mainTask.Price;
                    //CountGeneralCosts();
                    await DisplayAlert(null, "Επιτυχής Αλλαγή τιμής!", "OK");
                }
                else if (EditGeneralCostPicker.SelectedIndex == 1) // edit date
                {
                    AthensGeneralCostDatepicker.IsVisible = true;
                    AthensGeneralCostDatepicker.Focus();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void AthensGeneralCostDatepicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading();
                mainTask.Date = AthensGeneralCostDatepicker.Date;
                if (mainTask.Date < DateTime.Today)
                {
                    await DisplayAlert("Σφάλμα", "Επιλέξτε μία μελλοντική μέρα!", "OK");
                    return;
                }
                await App.ItemController.UpdateTask(mainTask);
                LastDayCostLabel.Text = mainTask.Date.ToString("MM/dd", CultureInfo.InvariantCulture); //MM/dd/yyyy
                AthensGeneralCostDatepicker.IsVisible = false;
                AthensGeneralCostDatepicker.Unfocus();
                currentTask = null;
                UserDialogs.Instance.HideLoading();
                await DisplayAlert(null, "Επιτυχής Αλλαγή Ημερομηνίας!", "OK");
                //AthensGeneralCostDatepicker.Date = DateTime.Today.AddDays(-1);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}