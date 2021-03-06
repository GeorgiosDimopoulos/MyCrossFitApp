﻿using System;
using Xamarin.Forms;
using MyItems.SQLite;
using MyItems.Views;
using MyItems.Views.TYPET;

namespace MyItems
{
	public partial class App : Application
	{
	    public static ItemDatabase ItemDatabase { get; set; }
        public static ISqliteManage SqliteManage { get; set; }
	    public static ItemController ItemController { get; set; }
        public static IIndicate Indicator { get; set; }

        public App ()
		{
            try
            {
                InitializeComponent();
                ItemDatabase = new ItemDatabase(SqliteManage.DatabaseFolder());
                ItemController = new ItemController();
                MainPage = new NavigationPage(new TypetPage());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
	    }
        
        public static void Init(ISqliteManage sqliteManage, IIndicate indicator)
	    {
            try
            {
                SqliteManage = sqliteManage;
                Indicator = indicator;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnStart ()
		{

        }
		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}
		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}