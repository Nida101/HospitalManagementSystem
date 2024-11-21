﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static HospitalManagementSystem.Patient;

namespace HospitalManagementSystem
{
    public class Patient
    {
        // Properties - https://www.w3schools.com/cs/cs_properties.php + https://www.webdevtutor.net/blog/c-sharp-class-property-method#google_vignette + https://dotnettutorials.net/lesson/properties-csharp/?utm_content=cmp-true
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ContactDetails { get; set; }
        public string NHSNumber { get; set; }
        public string HospitalNumber { get; set; }
        public int PatientID { get; set; }

        public static List<Patient> PatientList = new List<Patient>();

        public Patient() { } // Parameterless constructor (implicitly added if not explicitly defined)

        public Patient(int patientId, string firstName, string lastName) // Optional constructor for initialization
        {
            PatientID = patientId;
            FirstName = firstName;
            LastName = lastName;
        }



        // Constructor
        public Patient(string firstName, string lastName, DateTime dateOfBirth, string contactDetails, string nhsNumber, string hospitalNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            ContactDetails = contactDetails;
            NHSNumber = nhsNumber;
            HospitalNumber = hospitalNumber;
        }

        // Method to display patient details - https://wellsb.com/csharp/beginners/understanding-csharp-classes-and-methods/
        public void ViewPatient()
        {
            Console.WriteLine($"Name: {FirstName} {LastName}");
            Console.WriteLine($"Date of Birth: {DateOfBirth: yyyy-MM-dd}");
            Console.WriteLine($"Contact Details: {ContactDetails}");
            Console.WriteLine($"NHS Number: {NHSNumber}");
            Console.WriteLine($"Hospital Number: {HospitalNumber}");
            Console.WriteLine(new string('-', 30));
        }

        public static class PatientManager
        {
            private static List<Patient> patients = new List<Patient>();

            // Method to add a new patient
            public static void AddPatient(Patient patient)
            {
                patients.Add(patient);
                Console.WriteLine("Patient added successfully.");
            }

            // Method to view all patients
            public static void ListAllPatients()
            {
                if (patients.Count == 0)
                {
                    Console.WriteLine("No patients found.");
                    return;
                }
                Console.WriteLine("Patient List: ");
                foreach (var patient in patients)
                {
                    patient.ViewPatient();
                }
            }


            public static List<Patient> SearchPatientByDobAndName(DateTime dateOfBirth, string fullName)
            {
                return patients
                    .Where(p => p.DateOfBirth.Date == dateOfBirth.Date &&
                                $"{p.FirstName} {p.LastName}".Equals(fullName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            public static Patient SearchPatientByHospitalNumber(string hospitalNumber) //Method to search patient by Hospital Number
            {
                return patients.FirstOrDefault(p => p.HospitalNumber.Equals(hospitalNumber, StringComparison.OrdinalIgnoreCase));
            }

            public static Patient SearchPatientByNHSNumber(string nhsNumber) //Method to search patient by NHS Number
            {
                return patients.FirstOrDefault(p => p.NHSNumber.Equals(nhsNumber, StringComparison.OrdinalIgnoreCase));
            }

            public static Patient SearchPatientByNHSOrHospitalNumber(string identifier) //Method to search a patient using either NHS Number or Hospital Number
            {
                var patientByHospitalNumber = SearchPatientByHospitalNumber(identifier);
                if (patientByHospitalNumber != null)
                    return patientByHospitalNumber;

                var patientByNHSNumber = SearchPatientByNHSNumber(identifier);
                return patientByNHSNumber;
            }

            public static void ViewPatientDetails(Patient patient)
            {
                if (patient == null)
                {
                    Console.WriteLine("Patient not found.");
                    return;
                }

                Console.WriteLine($"Name: {patient.FirstName} {patient.LastName}");
                Console.WriteLine($"Date of Birth: {patient.DateOfBirth.ToShortDateString()}");
                Console.WriteLine($"Contact Detail: {patient.ContactDetails}");
                Console.WriteLine($"NHS Number: {patient.NHSNumber}");
                Console.WriteLine($"Hospital Number:{patient.HospitalNumber}");
                Console.WriteLine("Appointments: ");
                foreach (var appointment in patient.Appointments)
                {
                    Console.WriteLine($" Date: {appointment.AppointmentDate}");
                    Console.WriteLine($" Doctor: {appointment.DoctorName}");
                    Console.WriteLine($" Department: {appointment.Department}");
                    Console.WriteLine();
                }
            }

            public static void UpdatePatientDetails(string firstName, string lastName, DateTime dateOfBirth, string contactDetail, string nhsNumber, string hospitalNumber)
            {
                var patient = SearchPatientByNHSNumber(nhsNumber);
                if (patient != null)
                {
                    patient.FirstName = firstName;
                    patient.LastName = lastName;
                    patient.DateOfBirth = dateOfBirth;
                    patient.ContactDetails = contactDetail;
                    patient.NHSNumber = nhsNumber;
                    patient.HospitalNumber = hospitalNumber;
                }
            }

            public static void AddAppointment(string nhsNumber, Appointment appointment)
            {
                var patient = SearchPatientByNHSNumber(nhsNumber);
                if (patient != null)
                {
                    patient.Appointments.Add(appointment);
                }
            }

            public static void CancelAppointment(string nhsNumber, DateTime appointmentDate)
            {
                var patient = SearchPatientByNHSNumber(nhsNumber);
                if (patient != null)
                {
                    var appointment = patient.Appointments.FirstOrDefault(a => a.AppointmentDate == appointmentDate);
                    if (appointment != null)
                    {
                        appointment.AppointmentCancelled = true;
                    }
                }
            }

            public static void AddPrescription(string nhsNumber, Prescription prescription)
            {
                var patient = SearchPatientByNHSNumber(nhsNumber);
                if (patient != null)
                {
                    patient.Prescriptions.Add(prescription);
                }
            }

            public static void AddNote(string nhsNumber, Notes note)
            {
                var patient = SearchPatientByNHSNumber(nhsNumber);
                if (patient != null)
                {
                    patient.Note.Add(note);
                }
            }

            public static List<Notes> GetNotes(string nhsNumber)
            {
                var patient = SearchPatientByNHSNumber(nhsNumber);
                if (patient != null)
                {
                    return patient.Note.OrderByDescending(note => note.DateCreated).ToList();
                }
                return new List<Notes>();
            }            
        }

        public class PatientService
        {
            private readonly Dictionary<string, int> dailyPatientCount = new Dictionary<string, int>(); //2.1 

            public Patient CreatePatient(string firstName, string lastName, DateTime dateOfBirth, string contactDetails, string nhsNumber, string hospitalNumber)
            {
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(contactDetails) || string.IsNullOrWhiteSpace(nhsNumber) || string.IsNullOrWhiteSpace(hospitalNumber))
                {
                    throw new ArgumentException("All fields are mandatory.");
                }
                if (string.IsNullOrWhiteSpace(hospitalNumber))
                {
                    hospitalNumber = GenerateHospitalNumber();
                }


                var patient = new Patient(firstName, lastName, dateOfBirth, contactDetails, nhsNumber, hospitalNumber);

                return patient;
            }


            public string GenerateHospitalNumber()
            {
                string today = DateTime.Now.ToString("ddMMyy");

                if (!dailyPatientCount.ContainsKey(today))
                {
                    dailyPatientCount[today] = 1;
                }
                else
                {
                    dailyPatientCount[today]++;
                }

                string sequenceNumber = dailyPatientCount[today].ToString("D2");
                return ($"PRS-{today}-{sequenceNumber}");
            }


        }

        public class Appointment //2.2
        {
            public DateTime AppointmentDate { get; set; }
            public int AppointmentID { get; set; }
            public string DoctorName { get; set; }
            public int DoctorID { get; set; }
            public string Department { get; set; }
            public bool AppointmentCancelled { get; set; }
            public int PatientID { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

        }
        public List<Appointment> Appointments { get; set; } = new List<Appointment>(); //Adding a property for Appointment

        public class Prescription
        {
            public string Medication { get; set; }
            public string Dosage { get; set; }
            public DateTime DatePrescribed { get; set; }
        }
        public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();

        public class Notes
        {
            public DateTime DateCreated { get; set; }
            public string Content { get; set; }
        }
        public List<Notes> Note { get; set; } = new List<Notes>();


        // Input for 2.3 
        public static string PromptForNHSNumber()
        {
            Console.Write("Enter NHS Number: ");
            string nhsNumber = Console.ReadLine();

            // Validate input
            while (string.IsNullOrWhiteSpace(nhsNumber))
            {
                Console.WriteLine("NHS Number cannot be empty. Please try again.");
                Console.Write("Enter NHS Number: ");
                nhsNumber = Console.ReadLine();
            }

            return nhsNumber;
        }

        public static Patient GetPatientByNHSNumber()
        {
            string nhsNumber = PromptForNHSNumber(); // Prompt the user for the NHS number
            Patient patient = PatientManager.SearchPatientByNHSNumber(nhsNumber); // Search for the patient

            if (patient == null)
            {
                Console.WriteLine("No patient found with the given NHS Number.");
            }

            return patient; // Returns null if no patient is found
        }

        public static DateTime PromptForDate(string promptMessage)
        {
            Console.Write(promptMessage);
            string input = Console.ReadLine();

            DateTime parsedDate;

            // Validate and parse the input
            while (!DateTime.TryParse(input, out parsedDate))
            {
                Console.WriteLine("Invalid date format. Please enter a valid date (e.g., yyyy-MM-dd HH:mm).");
                Console.Write(promptMessage);
                input = Console.ReadLine();
            }

            return parsedDate;
        }


        // Second Part for 2.3 
        public class Doctor
        {
            public int DoctorID { get; set; }
            public string Name { get; set; }
        }

        public static class DoctorManager
        {
            // A collection to store doctors
            private static List<Doctor> doctors = new List<Doctor>();

            // Method to add a doctor to the list
            public static void Add(Doctor doctor)
            {
                if (doctor == null)
                {
                    throw new ArgumentNullException(nameof(doctor), "Doctor cannot be null.");
                }

                if (doctors.Any(d => d.DoctorID == doctor.DoctorID))
                {
                    Console.WriteLine("Doctor with this ID already exists.");
                }
                else
                {
                    doctors.Add(doctor);
                    Console.WriteLine($"Doctor {doctor.Name} added successfully.");
                }
            }

            // Method to retrieve the list of doctors
            public static List<Doctor> GetDoctors()
            {
                return new List<Doctor>(doctors); // Return a copy of the list
            }
        }


        public class AppointmentScheduler
        {
            private static List<Appointment> appointments = new List<Appointment>();

            public static bool ScheduleAppointment(int DoctorID, int PatientID, DateTime startTime, DateTime endTime)
            {
                if (DoctorID <= 0) // ensure a valid doctor is assigned
                {
                    throw new ArgumentException("A valid doctor must be assigned to the appointment.");
                }

                if (IsConflict(DoctorID, PatientID, startTime, endTime)) // prevent scheduling conflicts 
                {
                    throw new InvalidOperationException("The appointment conflicts with an existing schedule.");
                }

                var appointment = new Appointment // schedule the appointment
                {
                    AppointmentID = GeneratedAppointmentID(),
                    DoctorID = DoctorID,
                    PatientID = PatientID,
                    StartTime = startTime,
                    EndTime = endTime
                };

                appointments.Add(appointment);
                return true;
            }

            public static List<Appointment> GetAppointments()
            {
                return new List<Appointment>(appointments); // Return a copy of the appointments list
            }


            private static bool IsConflict(int DoctorID, int PatientID, DateTime startTime, DateTime endTime)
            {
                return appointments.Any(a =>
                    (a.DoctorID == DoctorID || a.PatientID == PatientID) &&
                    ((startTime >= a.StartTime && startTime < a.EndTime) ||
                     (endTime > a.StartTime && endTime <= a.EndTime) ||
                     (startTime <= a.StartTime && endTime >= a.EndTime)));
            }


            public static int GeneratedAppointmentID()
            {
                return new Random().Next(1, 10000); // random ID generator
            }

            public static void ListAppointment()
            {
                if (appointments.Count == 0)
                {
                    Console.WriteLine("No appointments scheduled.");
                    return;
                }

                foreach (var appointment in appointments)
                {
                    Console.WriteLine($"Appointment ID: {appointment.AppointmentID}, Doctor ID: {appointment.DoctorID}, Patient ID: {appointment.PatientID}, Start: {appointment.StartTime}, End: {appointment.EndTime}");
                }
            }

            internal static void AddAppointment(Appointment appointment)
            {
                throw new NotImplementedException();
            }

            public static void CancelAppointment(int appointmentId) // New method added
            {
                var appointment = appointments.FirstOrDefault(a => a.AppointmentID == appointmentId);
                if (appointment == null)
                {
                    throw new ArgumentException($"No appointment found with ID {appointmentId}.");
                }

                appointments.Remove(appointment);
                Console.WriteLine($"Appointment with ID {appointmentId} has been canceled successfully.");
            }
        }
    }
}