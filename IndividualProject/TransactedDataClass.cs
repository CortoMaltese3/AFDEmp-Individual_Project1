﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace IndividualProject
{
    class TransactedDataClass
    {
        static readonly string currentUsername = ConnectToServerClass.RetrieveCurrentLoginCredentialsFromDatabase();
        static readonly string connectionString = $"Server=localhost; Database = Project1_Individual; User Id = admin; Password = admin";
        static readonly string newTechnicalIssuePath = @"C:\Users\giorg\Documents\Coding\AFDEmp\C#\Individual Project 1\CRMTickets\TechnicalIssues";

        public static void ManageCustomerTickets()
        {
            //InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            //InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            ConsoleKey option = InputOutputAnimationControlClass.ManageTicketOptionsSreen();

            switch (option)
            {
                case ConsoleKey.D1:
                    OpenNewCustomerTicket();
                    break;

                case ConsoleKey.D2:
                    CloseCustomerTicket();
                    break;

                case ConsoleKey.Escape:
                    ActiveUserFunctionsClass.ActiveUserProcedures();
                    break;
            }
        }

        public static void OpenCustomerTickets()
        {
            ConsoleKey option = InputOutputAnimationControlClass.ManageTicketOptionsSreenAsUser();

            switch (option)
            {
                case ConsoleKey.D1:
                    OpenNewCustomerTicket();
                    break;

                case ConsoleKey.Escape:
                    ActiveUserFunctionsClass.ActiveUserProcedures();
                    break;
            }
        }

        public static string AssignTicketToUser()
        {
            Console.WriteLine("\r\nWould you like to assign the ticket to another user?");
            string option = InputOutputAnimationControlClass.PromptYesOrNo();
            if (option == "Y" || option == "y")
            {
                InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");

                Dictionary<string, string> AvailableUsernamesDictionary = RoleFunctionsClass.ShowAvailableUsersFromDatabase();
                Console.Write("\r\nPlease select a user and proceed to assign: ");
                string usernameAssignment = InputOutputAnimationControlClass.UsernameInput();

                while (AvailableUsernamesDictionary.ContainsKey(usernameAssignment) == false || usernameAssignment == "admin")
                {
                    if (AvailableUsernamesDictionary.ContainsKey(usernameAssignment) == false)
                    {
                        Console.WriteLine($"Database does not contain a User {usernameAssignment}");
                        System.Threading.Thread.Sleep(2000);
                        InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                        AvailableUsernamesDictionary = RoleFunctionsClass.ShowAvailableUsersFromDatabase();
                        Console.Write("\r\nPlease select a user and proceed to assign: ");
                        usernameAssignment = InputOutputAnimationControlClass.UsernameInput();
                    }
                    else
                    {
                        Console.WriteLine("Cannot assign ticket to super_admin! Please choose a different user");
                        System.Threading.Thread.Sleep(2000);
                        InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                        AvailableUsernamesDictionary = RoleFunctionsClass.ShowAvailableUsersFromDatabase();
                        Console.Write("\r\nPlease select a user and proceed to assign: ");
                        usernameAssignment = InputOutputAnimationControlClass.UsernameInput();
                    }
                }
                return usernameAssignment;
            }
            return currentUsername;
        }

        public static void OpenNewCustomerTicket()
        {
            string comment = InputOutputAnimationControlClass.TicketComment();
            string userAssignedTo = AssignTicketToUser();

            using (SqlConnection dbcon = new SqlConnection(connectionString))
            {
                dbcon.Open();
                SqlCommand openNewTechnicalTicket = new SqlCommand($"INSERT INTO CustomerTickets VALUES (GETDATE(), '{currentUsername}', '{userAssignedTo}', 'open', '{comment}')", dbcon);
                openNewTechnicalTicket.ExecuteScalar();
                SqlCommand fetchNewTicketID = new SqlCommand($"SELECT TOP 1 ticketID FROM CustomerTickets ORDER BY ticketID DESC", dbcon);
                int ticketID = (int)fetchNewTicketID.ExecuteScalar();
                InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                InputOutputAnimationControlClass.UniversalLoadingOuput("Filing new customer ticket in progress");
                Console.WriteLine($"New Customer Ticket with ID: {ticketID} has been successfully created and assigned to {userAssignedTo}. Status: Open");
            }
            Console.WriteLine("\r\nPress any key to return");
            Console.ReadKey();
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            ManageCustomerTickets();
        }

        public static void CloseCustomerTicket()
        {
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            Console.WriteLine("CLOSE EXISTING TECHNICAL TICKETS");

            Console.WriteLine("Would you like to open the list of Opened Tickets?");
            string option = InputOutputAnimationControlClass.PromptYesOrNo();
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");

            using (SqlConnection dbcon = new SqlConnection(connectionString))
            {
                if (option == "Y" || option == "y")
                {
                    ViewListOfOpenCustomerTickets();
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                    InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                    InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
                    ManageCustomerTickets();
                }
                else
                {
                    int ticketID = InputOutputAnimationControlClass.SelectTicketID();
                    if (CheckIfTicketIDWithStatusOpenExistsInList(ticketID) == false)
                    {
                        Console.WriteLine($"There is no Customer Ticket with [ID = {ticketID}]");
                        System.Threading.Thread.Sleep(1500);
                        CloseCustomerTicket();
                    }
                    Console.WriteLine($"Are you sure you want to mark ticket {ticketID} as closed?");
                    string option2 = InputOutputAnimationControlClass.PromptYesOrNo();
                    if (option2 == "Y" || option2 == "y")
                    {
                        dbcon.Open();
                        SqlCommand closeCustomerTicket = new SqlCommand($"UPDATE CustomerTickets SET ticketStatus = 'closed' WHERE ticketID = {ticketID} ", dbcon);
                        closeCustomerTicket.ExecuteScalar();
                        InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                        InputOutputAnimationControlClass.UniversalLoadingOuput("Action in progress");
                        Console.WriteLine($"Customer ticket with CustomerID = {ticketID} has been successfully marked as closed");
                        System.Threading.Thread.Sleep(2000);
                    }
                    else
                    {
                        ManageCustomerTickets();
                    }
                }
            }
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            ManageCustomerTickets();
        }

        public static void DeleteExistingOpenOrClosedTicketFunction()
        {
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            Console.WriteLine("DELETE EXISTING TECHNICAL TICKETS");

            Console.WriteLine("Would you like to open the list of Existing Tickets?");
            string option = InputOutputAnimationControlClass.PromptYesOrNo();
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");

            if (option == "Y" || option == "y")
            {
                ViewListOfAllCustomerTickets();
                Console.Write("Press any key to continue");
                Console.ReadKey();
                DeleteExistingOpenOrClosedTicketFunction();
            }
            else
            {
                int ticketID = InputOutputAnimationControlClass.SelectTicketID();
                if (CheckIfTicketIDWithStatusOpenOrClosedExistsInList(ticketID) == false)
                {
                    Console.WriteLine($"There is no Customer Ticket with [ID = {ticketID}]");
                    System.Threading.Thread.Sleep(1500);
                    ActiveUserFunctionsClass.ActiveUserProcedures();
                }
                Console.WriteLine($"Are you sure you want to delete ticket {ticketID}? Action cannot be undone");
                string option2 = InputOutputAnimationControlClass.PromptYesOrNo();
                if (option2 == "Y" || option2 == "y")
                {
                    using (SqlConnection dbcon = new SqlConnection(connectionString))
                    {
                        dbcon.Open();
                        SqlCommand deleteCustomerTicket = new SqlCommand($"DELETE FROM CustomerTickets WHERE ticketID = {ticketID}", dbcon);
                        deleteCustomerTicket.ExecuteScalar();
                        InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                        InputOutputAnimationControlClass.UniversalLoadingOuput("Action in progress");
                        Console.WriteLine($"Customer ticket with CustomerID = {ticketID} has been successfully deleted");
                        System.Threading.Thread.Sleep(2000);
                    }
                }
            }
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            ActiveUserFunctionsClass.ActiveUserProcedures();
        }

        public static void ViewExistingOpenTicketsFunction()
        {
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            Console.WriteLine("VIEW OPEN TECHNICAL TICKETS");

            Console.WriteLine("Would you like to open the list of Opened Tickets?");
            string option = InputOutputAnimationControlClass.PromptYesOrNo();
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");

            if (option == "Y" || option == "y")
            {
                ViewListOfOpenCustomerTickets();
            }

            int TicketID = InputOutputAnimationControlClass.SelectTicketID();
            if (CheckIfTicketIDWithStatusOpenExistsInList(TicketID) == false)
            {
                Console.WriteLine($"There is no Customer Ticket with [ID = {TicketID}]");
                System.Threading.Thread.Sleep(1500);
                ViewExistingOpenTicketsFunction();
            }

            ViewSingleCustomerTicket(TicketID);
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            ActiveUserFunctionsClass.ActiveUserProcedures();
        }

        private static void ViewSingleCustomerTicket(int ticketID)
        {
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            Console.WriteLine($"VIEW TECHNICAL TICKET WITH [ID = {ticketID}]");

            using (SqlConnection dbcon = new SqlConnection(connectionString))
            {
                dbcon.Open();
                SqlCommand ShowTicketsFromDatabase = new SqlCommand($"SELECT * FROM CustomerTickets WHERE ticketID = {ticketID}", dbcon);
                using (var reader = ShowTicketsFromDatabase.ExecuteReader())
                {
                    List<string> ShowtTicketToList = new List<string>();
                    while (reader.Read())
                    {
                        int ID = (int)reader[0];
                        DateTime dateCreated = (DateTime)reader[1];
                        string username = (string)reader[2];
                        string userAssignedTo = (string)reader[3];
                        string ticketStatus = (string)reader[4];
                        string comments = (string)reader[5];

                        ShowtTicketToList.Add(ticketID.ToString());
                        ShowtTicketToList.Add(dateCreated.ToString());
                        ShowtTicketToList.Add(username);
                        ShowtTicketToList.Add(userAssignedTo);
                        ShowtTicketToList.Add(ticketStatus);
                        ShowtTicketToList.Add(comments);
                        Console.WriteLine($"TicketID: {ticketID} \r\nDate created: {dateCreated} \r\nCreated By: {username} \r\nAssigned To: {userAssignedTo} \r\nTicket status: {ticketStatus} \r\bComment preview: {comments}");
                        Console.WriteLine(new string('#', Console.WindowWidth));
                    }
                }
                Console.Write("Press any key to return");
                Console.ReadKey();
            }
        }

        private static void ViewListOfOpenCustomerTickets()
        {
            using (SqlConnection dbcon = new SqlConnection(connectionString))
            {
                dbcon.Open();
                SqlCommand ShowTicketsFromDatabase = new SqlCommand("SELECT * FROM CustomerTickets WHERE ticketStatus = 'open'", dbcon);
                using (var reader = ShowTicketsFromDatabase.ExecuteReader())
                {
                    List<string> ShowtTicketsList = new List<string>();
                    while (reader.Read())
                    {
                        int ticketID = (int)reader[0];
                        DateTime dateCreated = (DateTime)reader[1];
                        string username = (string)reader[2];
                        string userAssignedTo = (string)reader[3];
                        string ticketStatus = (string)reader[4];
                        string comments = (string)reader[5];
                        var stringLength = comments.Length;
                        if (stringLength > 60)
                        {
                            comments = comments.Substring(0, 60) + "...";
                        }

                        ShowtTicketsList.Add(ticketID.ToString());
                        ShowtTicketsList.Add(dateCreated.ToString());
                        ShowtTicketsList.Add(username);
                        ShowtTicketsList.Add(userAssignedTo);
                        ShowtTicketsList.Add(ticketStatus);
                        ShowtTicketsList.Add(comments);
                        Console.WriteLine($"TicketID: {ticketID} \r\nDate created: {dateCreated} \r\nCreated By: {username} \r\nAssigned To: {userAssignedTo} \r\nTicket status: {ticketStatus} \r\bComment preview: {comments}");
                        Console.WriteLine(new string('#', Console.WindowWidth));
                        Console.WriteLine();
                    }
                }
            }
        }

        private static void ViewListOfAllCustomerTickets()
        {
            using (SqlConnection dbcon = new SqlConnection(connectionString))
            {
                dbcon.Open();
                SqlCommand ShowTicketsFromDatabase = new SqlCommand("SELECT * FROM CustomerTickets", dbcon);
                using (var reader = ShowTicketsFromDatabase.ExecuteReader())
                {
                    List<string> ShowtTicketsList = new List<string>();
                    while (reader.Read())
                    {
                        int ticketID = (int)reader[0];
                        DateTime dateCreated = (DateTime)reader[1];
                        string username = (string)reader[2];
                        string userAssignedTo = (string)reader[3];
                        string ticketStatus = (string)reader[4];
                        string comments = (string)reader[5];
                        var stringLength = comments.Length;
                        if (stringLength > 40)
                        {
                            comments = comments.Substring(0, 40) + "...";
                        }

                        ShowtTicketsList.Add(ticketID.ToString());
                        ShowtTicketsList.Add(dateCreated.ToString());
                        ShowtTicketsList.Add(username);
                        ShowtTicketsList.Add(userAssignedTo);
                        ShowtTicketsList.Add(ticketStatus);
                        ShowtTicketsList.Add(comments);
                        Console.WriteLine($"TicketID: {ticketID} \r\nDate created: {dateCreated} \r\nCreated By: {username} \r\nAssigned To: {userAssignedTo} \r\nTicket status: {ticketStatus} \r\bComment preview: {comments}");
                        Console.WriteLine(new string('#', Console.WindowWidth));
                        Console.WriteLine();
                    }
                }
            }
        }

        public static void EditExistingOpenTicketFunction()
        {
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
            Console.WriteLine("EDIT OPEN TECHNICAL TICKETS");

            Console.WriteLine("Would you like to open the list of Opened Tickets?");
            string option = InputOutputAnimationControlClass.PromptYesOrNo();
            InputOutputAnimationControlClass.QuasarScreen(currentUsername);
            InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");

            if (option == "Y" || option == "y")
            {
                ViewListOfOpenCustomerTickets();
            }

            int TicketID = InputOutputAnimationControlClass.SelectTicketID();
            if (CheckIfTicketIDWithStatusOpenExistsInList(TicketID) == false)
            {
                Console.WriteLine($"There is no Customer Ticket with [ID = {TicketID}]");
                System.Threading.Thread.Sleep(1500);
                ActiveUserFunctionsClass.ActiveUserProcedures();
            }
            else
            {
                EditTicketOptions(TicketID);

                InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                Console.WriteLine($"Would you like to view the edited Ticket {TicketID}?");
                string option2 = InputOutputAnimationControlClass.PromptYesOrNo();
                if (option2 == "Y" || option2 == "y")
                {
                    ViewSingleCustomerTicket(TicketID);
                }
                InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
                ActiveUserFunctionsClass.ActiveUserProcedures();
            }
        }

        private static void EditTicketOptions(int ID)
        {
            ConsoleKey option = InputOutputAnimationControlClass.EditTicketScreenOptions();

            switch (option)
            {
                case ConsoleKey.D1:
                    string ticketComment = InputOutputAnimationControlClass.TicketComment();
                    EditCommentOfOpenTicket(ID, ticketComment);
                    break;

                case ConsoleKey.D2:
                    string newUserAssignment = AssignTicketToUser();
                    ChangeUserAssignmentToOpenTicket(ID, newUserAssignment);
                    break;

                case ConsoleKey.Escape:
                    InputOutputAnimationControlClass.QuasarScreen(currentUsername);
                    InputOutputAnimationControlClass.UniversalLoadingOuput("Loading");
                    ActiveUserFunctionsClass.ActiveUserProcedures();
                    break;
            }
        }

        private static void EditCommentOfOpenTicket(int ID, string ticketComment)
        {
            using (SqlConnection dbcon = new SqlConnection(connectionString))
            {
                dbcon.Open();
                SqlCommand EditTicketCommendInDatabase = new SqlCommand($"UPDATE CustomerTickets SET comments = '{ticketComment}' WHERE ticketID = {ID}", dbcon);
                EditTicketCommendInDatabase.ExecuteScalar();
            }
            Console.WriteLine($"The comment section of the Customer Ticket with [ID = {ID}] has been successfully edited");
            System.Threading.Thread.Sleep(2000);
        }

        private static void ChangeUserAssignmentToOpenTicket(int ID, string nextOwner)
        {
            using (SqlConnection dbcon = new SqlConnection(connectionString))
            {
                dbcon.Open();
                SqlCommand EditTicketUserOwnerInDatabase = new SqlCommand($"UPDATE CustomerTickets SET userAssignedTo = '{nextOwner}' WHERE ticketID = {ID}", dbcon);
                EditTicketUserOwnerInDatabase.ExecuteScalar();
            }
            Console.WriteLine($"The ownership of the Customer Ticket with [ID = {ID}] has been successfully transfered to {nextOwner}");
            System.Threading.Thread.Sleep(2000);
        }

        private static bool CheckIfTicketIDWithStatusOpenExistsInList(int ID)
        {
            using (SqlConnection dbcon = new SqlConnection(connectionString))
            {
                dbcon.Open();
                SqlCommand ShowTicketsFromDatabase = new SqlCommand("SELECT ticketID FROM CustomerTickets WHERE ticketStatus = 'open'", dbcon);
                using (var reader = ShowTicketsFromDatabase.ExecuteReader())
                {
                    List<string> ShowtTicketsList = new List<string>();
                    while (reader.Read())
                    {
                        int ticketID = (int)reader[0];
                        ShowtTicketsList.Add(ticketID.ToString());
                    }
                    if (ShowtTicketsList.Contains(ID.ToString()) == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CheckIfTicketIDWithStatusOpenOrClosedExistsInList(int ID)
        {
            using (SqlConnection dbcon = new SqlConnection(connectionString))
            {
                dbcon.Open();
                SqlCommand ShowTicketsFromDatabase = new SqlCommand("SELECT ticketID FROM CustomerTickets", dbcon);
                using (var reader = ShowTicketsFromDatabase.ExecuteReader())
                {
                    List<string> ShowtTicketsList = new List<string>();
                    while (reader.Read())
                    {
                        int ticketID = (int)reader[0];
                        ShowtTicketsList.Add(ticketID.ToString());
                    }
                    if (ShowtTicketsList.Contains(ID.ToString()) == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}