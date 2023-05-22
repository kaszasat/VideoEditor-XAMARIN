# VideoEditor

VideoEditor is a cross-platform video editing application developed using Xamarin for UWP, Android, and iOS platforms. It allows users to perform various video editing operations such as trimming, cropping, adding effects, and more. The project was initially developed as a final thesis for my Bachelor's degree and showcases the implementation of a video editing application using Xamarin.

## Technologies Used
 - C#
 - Python
 - .NET
 - Xamarin
 - REST API
 - MVVM
 - Android (Java)
 - iOS (Swift)
 - UWP (C#)
 - FFmpeg
 - PhotoShop
 - XML (XAML)
 - UML
 - Visio
 - GNU/Linux
 - Ubuntu
 - Flask (Python)
 - WSL (Windows Subsystem for Linux)

## Why?

The aim of this code was to design and implement a cross-platform video editing application using Xamarin for UWP, Android, and iOS platforms. The UWP implementation adheres to the originally set specifications, while modifications were made to the initial specifications for the iOS and Android platforms. These modifications involved outsourcing video processing operations to a development server. The original specifications are partly awailable if you know who I am and where to search for my final thesis.

At the time of the thesis, there were no existing cross-platform video editing applications for UWP, Android, and iOS platforms implemented using Xamarin. Hence, the project's implementation using Xamarin is considered sufficient as a final thesis. Although Xamarin support ended on May 1, 2024, its successor, .NET MAUI, provides a similar functionality and allows developers to migrate their code from Xamarin.Forms to .NET MAUI. The thesis includes a decision process and its results, justifying the use of Xamarin.Forms technologies during development instead of .NET MAUI.

The implementation of the project builds upon the knowledge acquired during my university bachelor's degree. It utilizes engineering and IT skills to solve an engineering problem, demonstrating my capabilities in solving real-world challenges.

In this project, we were able to gain insight into the design and implementation of a cross-platform video editing application using Xamarin for UWP, Android and iOS platforms. The user interface was realized according to the planned drawings and plans. After learning how FFmpeg works, the UWP implementation was implemented according to the original plans, while the Android and iOS implementation was implemented according to the revised plans and a development server processes the data. The development server is a GNU/Linux operating system with an Ubuntu distribution running on the GNU/Linux subsystem of a Windows 10 operating system. The server uses the Flask micro web framework to implement REST API functionality. The developer server for Android and iOS implements the same functionality as the UWP implementation, only with a different method. FFmpeg libraries are used for both approaches. We call the FFmpeg libraries directly on the server, while in the case of the UWP implementation we access FFmpeg via the Xabe.FFmpeg package. An Internet connection is required for both implementations: on UWP using Xabe.FFmpeg to download the FFmpeg binary executable, and on Android and iOS to access the development server on the local network.

The logical and user interface control elements of the program were separated from each other during the cross-platform development using the implementation according to the MVVM architecture. Looking at the implementation retrospectively, the waterfall model was suitable for the organization of project activities, it would have been an unnecessary overcomplication to use a more complex software development model, even with unexpected changes to the plans. The video editing application was completed as expected for the set specification, the development of the application was successfully completed.

## Usage 

To use the VideoEditor application, follow the instructions below:

1. Clone the repository: <code>git clone https://github.com/kaszasat/VideoEditor.git</code>
2. Open the solution file in Visual Studio or your preferred development environment.
3. Build the solution to restore NuGet packages and compile the code.
4. Select the desired platform (UWP, Android, or iOS) and deploy the application to the corresponding emulator or device.
5. Once the application is launched, you can explore the various video editing features and functionalities provided by VideoEditor.
6. (Optional) Configure the client (iOS, Android) and the server for the connection.
7. (Optional) Run the <code>VideoEditor/VideoEditor.Server/VideoEditor.Server.py/</code> in your GNU/Linux operating system.

## License
This project is licensed under the MIT License.

## Acknowledgments

I would like to express their gratitude to the faculty, advisors, and mentors who provided guidance and support throughout the development of this project. Special thanks to the open-source community and the contributors of the libraries and frameworks used in this project.
 - https://github.com/FFmpeg/FFmpeg
 - https://github.com/tomaszzmuda/Xabe.FFmpeg
 - https://nunit.org/
 - https://github.com/AlDanial/cloc
 - https://github.com/github/linguist
 - https://github.com/rochacbruno/flask-powered
 - https://www.ffmpeg.org/about.html
 - https://dotnet.microsoft.com/en-us/apps/xamarin

## Project Status
The VideoEditor project is no longer actively maintained. However, the code is available for reference and educational purposes.
The server runs on localhost. The server was never meant to be operated/accessed outside of a development environment, thus the code reflects that.
The code meant to demonstrate that a video editing application can be developed using Xamarin for UWP, Android, and iOS platforms.
