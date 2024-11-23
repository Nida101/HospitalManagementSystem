using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newton.Soft.Json;
using System.Text.Json;
using HospitalManagementSystem;
using static HospitalManagementSystem.Patient;

namespace HospitalManagementSystem
{

    class Program
    {
        static void Main()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine(new string('=', 50));
                Console.WriteLine("   WELCOME TO THE HOSPITAL MANAGEMENT SYSTEM   ");
                Console.WriteLine(new string('=', 50));
                Console.WriteLine(new string('-', 50));
                Console.WriteLine("MAIN MENU");
                Console.WriteLine(new string('-', 50));
                Console.WriteLine("1. Add Patient");
                Console.WriteLine("2. View or Update Patient Record");
                Console.WriteLine("3. Search Patient");
                Console.WriteLine("4. Manage Appointments");
                Console.WriteLine("5. Exit");
                Console.WriteLine(new string('-', 50));
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Clear();
                        Console.WriteLine(new string('-', 50));
                        Console.WriteLine("ADD PATIENT");
                        Console.WriteLine(new string('-', 50));
                        var patientService = new PatientService();

                        Console.Write("Enter First Name: ");
                        string firstName = Console.ReadLine();

                        Console.Write("Enter Last Name: ");
                        string lastName = Console.ReadLine();

                        Console.Write("Enter Date of Birth (yyyy-mm-dd): ");
                        if (!DateTime.TryParse(Console.ReadLine(), out DateTime dateOfBirth))
                        {
                            Console.WriteLine("Invalid date format. Please enter the date in yyyy-mm-dd format.");
                            break;
                        }

                        Console.Write("Enter Contact Details: ");
                        string contactDetails = Console.ReadLine();

                        Console.Write("Enter NHS Number: ");
                        string nhsNumber = Console.ReadLine();

                        string hospitalNumber = patientService.GenerateHospitalNumber();

                        var newPatient = patientService.CreatePatient(firstName, lastName, dateOfBirth, contactDetails, nhsNumber, hospitalNumber);

                        Console.WriteLine($"\nPatient {newPatient.FirstName} {newPatient.LastName} created with Hospital Number: {newPatient.HospitalNumber}");
                        break;

                    case "2":
                        
                        while (true)
                        {
                            Console.WriteLine("\nSelect an option:");
                            Console.WriteLine("1. View Patient Record");
                            Console.WriteLine("2. Update Patient Basic Details");
                            Console.WriteLine("3. Add Appointment");
                            Console.WriteLine("4. Cancel Appointment");
                            Console.WriteLine("5. Add Prescription");
                            Console.WriteLine("6. Add Note");
                            Console.WriteLine("7. View Notes");
                            Console.WriteLine("8. Exit to Main Menu");
                            Console.Write("Enter your choice: ");
                            string subChoice = Console.ReadLine();

                            switch (subChoice)
                            {
                                case "1":
                                    Console.Clear();
                                    Console.WriteLine(new string('-', 50));
                                    Console.WriteLine("VIEW OR UPDATE PATIENT RECORD");
                                    Console.WriteLine(new string('-', 50));
                                    PatientManager.ListAllPatients();
                                    break;

                                case "2":
                                    Console.Clear();
                                    Console.WriteLine(new string('-', 50));
                                    Console.WriteLine("SEARCH PATIENT");
                                    Console.WriteLine(new string('-', 50));
                                    Console.WriteLine("\nSearch from the following: ");
                                    Console.WriteLine("1. Full Name and Date of Birth");
                                    Console.WriteLine("2. NHS Number");
                                    Console.WriteLine("3. Hospital Number");
                                    Console.Write("Enter your choice: ");
                                    string searchChoice = Console.ReadLine();

                                    switch (searchChoice)
                                    {
                                        case "1":
                                            Console.Write("Enter Full Name (FirstName LastName): ");
                                            string fullName = Console.ReadLine();

                                            Console.Write("Enter Date of Birth (yyyy-mm-dd): ");
                                            if (!DateTime.TryParse(Console.ReadLine(), out DateTime dob))
                                            {
                                                Console.WriteLine("Invalid date format.");
                                                break;
                                            }

                                            var foundPatients = PatientManager.SearchPatientByDobAndName(dob, fullName);

                                            if (foundPatients.Any())
                                            {
                                                foreach (var patient in foundPatients)
                                                {
                                                    DisplayPatientInfo(patient);
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("No patient found with the given details.");
                                            }
                                            break;

                                        case "2":
                                            Console.Write("Enter NHS Number: ");
                                            string nhsNum = Console.ReadLine();

                                            var patientByNHS = PatientManager.SearchPatientByNHSNumber(nhsNum);

                                            if (patientByNHS != null)
                                            {
                                                DisplayPatientInfo(patientByNHS);
                                            }
                                            else
                                            {
                                                Console.WriteLine("No patient found with the given NHS Number.");
                                            }
                                            break;

                                        case "3":
                                            Console.Write("Enter Hospital Number: ");
                                            string hospitalNum = Console.ReadLine();

                                            var patientByHospital = PatientManager.SearchPatientByHospitalNumber(hospitalNum);

                                            if (patientByHospital != null)
                                            {
                                                DisplayPatientInfo(patientByHospital);
                                            }
                                            else
                                            {
                                                Console.WriteLine("No patient found with the given Hospital Number.");
                                            }
                                            break;

                                        default:
                                            Console.WriteLine("Invalid Choice - Please select a valid option.");
                                            break;
                                    }
                                    break;

                                case "3":
                                    var nhsForAppointment = PromptForNHSNumber();
                                    var appointmentDate = PromptForDate("Enter Appointment Date (yyyy-MM-dd HH:mm): ");
                                    Console.Write("Enter Doctor's Name: ");
                                    string doctor = Console.ReadLine();
                                    Console.Write("Enter Department: ");
                                    string department = Console.ReadLine();
                                    Console.Write("Enter Notes: ");
                                    string notes = Console.ReadLine();

                                    try
                                    {
                                        var appointment = new Appointment
                                        {
                                            AppointmentDate = appointmentDate,
                                            DoctorName = doctor,
                                            Department = department,
                                            AppointmentCancelled = false
                                        };
                                        PatientManager.AddAppointment(nhsForAppointment, appointment);
                                        Console.WriteLine("Appointment added successfully.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error adding appointment: {ex.Message}");
                                    }
                                    break;

                                case "4":
                                    var nhsForCancel = PromptForNHSNumber();
                                    var cancelDate = PromptForDate("Enter Appointment Date to Cancel (yyyy-MM-dd HH:mm): ");
                                    try
                                    {
                                        PatientManager.CancelAppointment(nhsForCancel, cancelDate);
                                        Console.WriteLine("Appointment cancelled successfully.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error cancelling appointment: {ex.Message}");
                                    }
                                    break;

                                case "5":
                                    var nhsForPrescription = PromptForNHSNumber();
                                    Console.Write("Enter Medication Name: ");
                                    string medication = Console.ReadLine();
                                    Console.Write("Enter Dosage: ");
                                    string dosage = Console.ReadLine();
                                    try
                                    {
                                        var prescription = new Prescription
                                        {
                                            Medication = medication,
                                            Dosage = dosage,
                                            DatePrescribed = DateTime.Now
                                        };
                                        PatientManager.AddPrescription(nhsForPrescription, prescription);
                                        Console.WriteLine("Prescription added successfully.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error adding prescription: {ex.Message}");
                                    }
                                    break;

                                case "6":
                                    var nhsForNote = PromptForNHSNumber();
                                    Console.Write("Enter Note Content: ");
                                    string content = Console.ReadLine();

                                    var newNote = new Notes
                                    {
                                        DateCreated = DateTime.Now,
                                        Content = content
                                    };

                                    var PatientService = new PatientService();
                                    PatientService.AddNoteToPatient(nhsForNote, newNote);
                                    break;

                                case "7":
                                    var nhsForViewNotes = PromptForNHSNumber();
                                    var notesList = PatientService.GetNotesForPatient(nhsForViewNotes);
                                    if (notesList.Any())
                                    {
                                        Console.WriteLine("\nPatient Notes:");
                                        foreach (var note in notesList)
                                        {
                                            Console.WriteLine($"Date: {note.DateCreated}");
                                            Console.WriteLine($"Content: {note.Content}");
                                            Console.WriteLine(new string('-', 30));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("\nNo notes found for this patient.");
                                    }
                                    break;

                                case "8":
                                    Console.WriteLine("Exiting to main menu.");
                                    return;

                                default:
                                    Console.WriteLine("Invalid choice. Please select a valid option.");
                                    break;
                            }
                        }
                        break;

                    case "3":
                        Console.WriteLine("\nSearch from the following: ");
                        Console.WriteLine("1. Full Name and Date of Birth");
                        Console.WriteLine("2. NHS Number");
                        Console.WriteLine("3. Hospital Number");
                        Console.Write("Enter your choice: ");
                        string searchChoiceMain = Console.ReadLine();

                        switch (searchChoiceMain)
                        {
                            case "1":
                                Console.Write("Enter Full Name (FirstName LastName): ");
                                string fullName = Console.ReadLine();

                                Console.Write("Enter Date of Birth (yyyy-mm-dd): ");
                                if (!DateTime.TryParse(Console.ReadLine(), out DateTime dob))
                                {
                                    Console.WriteLine("Invalid date format.");
                                    break;
                                }

                                var foundPatients = PatientManager.SearchPatientByDobAndName(dob, fullName);

                                if (foundPatients.Any())
                                {
                                    foreach (var patient in foundPatients)
                                    {
                                        DisplayPatientInfo(patient);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No patient found with the given details.");
                                }
                                break;

                            case "2":
                                Console.Write("Enter NHS Number: ");
                                string nhsNum = Console.ReadLine();

                                var patientByNHS = PatientManager.SearchPatientByNHSNumber(nhsNum);

                                if (patientByNHS != null)
                                {
                                    DisplayPatientInfo(patientByNHS);
                                }
                                else
                                {
                                    Console.WriteLine("No patient found with the given NHS Number.");
                                }
                                break;

                            case "3":
                                Console.Write("Enter Hospital Number: ");
                                string hospitalNum = Console.ReadLine();

                                var patientByHospital = PatientManager.SearchPatientByHospitalNumber(hospitalNum);

                                if (patientByHospital != null)
                                {
                                    DisplayPatientInfo(patientByHospital);
                                }
                                else
                                {
                                    Console.WriteLine("No patient found with the given Hospital Number.");
                                }
                                break;

                            default:
                                Console.WriteLine("Invalid choice. Please select a valid option.");
                                break;
                        }
                        break;

                    case "4":
                        Console.Clear();
                        Console.WriteLine(new string('-', 50));
                        Console.WriteLine("MANAGE APPOINTMENTS");
                        Console.WriteLine(new string('-', 50));
                        Console.WriteLine("1. Add Doctor");
                        Console.WriteLine("2. Add Patient");
                        Console.WriteLine("3. Schedule Appointment");
                        Console.WriteLine("4. List Appointments");
                        Console.WriteLine("5. Cancel Appointment");
                        Console.WriteLine("6. Exit");
                        Console.Write("Select an option: ");

                        string appointmentChoice = Console.ReadLine();

                        switch (appointmentChoice)
                        {
                            case "1": // Add Doctor
                                Console.Write("Enter Doctor ID: ");
                                if (int.TryParse(Console.ReadLine(), out int doctorId))
                                {
                                    Console.Write("Enter Doctor Name: ");
                                    string name = Console.ReadLine();
                                    DoctorManager.Add(new Doctor { DoctorID = doctorId, Name = name });
                                    Console.WriteLine("Doctor added successfully.");
                                }
                                else
                                {
                                    Console.WriteLine("Invalid ID. Doctor not added.");
                                }
                                Console.WriteLine("Press any key to return to the menu.");
                                Console.ReadKey();
                                break;

                            case "2": // Add Patient
                                Console.Write("Enter Patient ID: ");
                                if (int.TryParse(Console.ReadLine(), out int patientId))
                                {
                                    Console.Write("Enter Patient Name: ");
                                    string patientName = Console.ReadLine();
                                    PatientList.Add(new Patient { PatientID = patientId });
                                    Console.WriteLine("Patient added successfully.");
                                }
                                else
                                {
                                    Console.WriteLine("Invalid ID. Patient not added.");
                                }
                                Console.WriteLine("Press any key to return to the menu.");
                                Console.ReadKey();
                                break;

                            case "3": // Schedule Appointment
                                Console.Write("Enter Doctor ID: ");
                                while (!int.TryParse(Console.ReadLine(), out doctorId))
                                {
                                    Console.WriteLine("Invalid input. Please enter a valid Doctor ID.");
                                }

                                Console.Write("Enter Patient ID: ");
                                while (!int.TryParse(Console.ReadLine(), out patientId))
                                {
                                    Console.WriteLine("Invalid input. Please enter a valid Patient ID.");
                                }

                                Console.Write("Enter Start Time (yyyy-MM-dd HH:mm): ");
                                DateTime startTime;
                                while (!DateTime.TryParse(Console.ReadLine(), out startTime))
                                {
                                    Console.WriteLine("Invalid input. Please enter a valid date and time.");
                                }

                                Console.Write("Enter End Time (yyyy-MM-dd HH:mm): ");
                                DateTime endTime;
                                while (!DateTime.TryParse(Console.ReadLine(), out endTime))
                                {
                                    Console.WriteLine("Invalid input. Please enter a valid date and time.");
                                }

                                AppointmentScheduler.ScheduleAppointment(doctorId, patientId, startTime, endTime);
                                Console.WriteLine("Appointment scheduled successfully.");
                                break;

                            case "4": // List Appointments
                                var appointments = AppointmentScheduler.GetAppointments();
                                if (appointments.Any())
                                {
                                    Console.WriteLine("Scheduled Appointments:");
                                    foreach (var appointment in appointments)
                                    {
                                        Console.WriteLine($"Appointment ID: {appointment.AppointmentID}, Doctor ID: {appointment.DoctorID}, Patient ID: {appointment.PatientID}");
                                        Console.WriteLine($"Start Time: {appointment.StartTime}, End Time: {appointment.EndTime}");
                                        Console.WriteLine(new string('-', 30));
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No appointments scheduled.");
                                }
                                Console.WriteLine("Press any key to return to the menu.");
                                Console.ReadKey();
                                break;

                            case "5": // Cancel Appointment
                                Console.Write("Enter Appointment ID to cancel: ");
                                if (int.TryParse(Console.ReadLine(), out int appointmentId))
                                {
                                    try
                                    {
                                        AppointmentScheduler.CancelAppointment(appointmentId);
                                        Console.WriteLine("Appointment canceled successfully.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error: {ex.Message}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Invalid Appointment ID.");
                                }
                                Console.WriteLine("Press any key to return to the menu.");
                                Console.ReadKey();
                                break;

                            case "6": // Exit
                                Console.WriteLine("Exiting Appointment Scheduling System.");
                                break;

                            default:
                                Console.WriteLine("Invalid choice. Please select a valid option.");
                                break;
                        }
                        break;

                }

                static string PromptForNHSNumber()
                {
                    Console.Write("Enter NHS Number: ");
                    return Console.ReadLine();
                }

                static DateTime PromptForDate(string promptMessage)
                {
                    DateTime date = default;
                    Console.Write(promptMessage);
                    while (!DateTime.TryParse(Console.ReadLine(), out date))
                    {
                        Console.WriteLine("Invalid date format. Please try again.");
                        Console.Write(promptMessage);
                    }
                    return date;
                }

                static void DisplayPatientInfo(Patient patient)
                {
                    Console.WriteLine("\nPatient Information:");
                    Console.WriteLine($"Name: {patient.FirstName} {patient.LastName}");
                    Console.WriteLine($"Date of Birth: {patient.DateOfBirth:yyyy-MM-dd}");
                    Console.WriteLine($"Contact Details: {patient.ContactDetails}");
                    Console.WriteLine($"NHS Number: {patient.NHSNumber}");
                    Console.WriteLine($"Hospital Number: {patient.HospitalNumber}");
                    Console.WriteLine(new string('-', 30));
                }
            }
        }
    }
}
