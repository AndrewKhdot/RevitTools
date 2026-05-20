Revit MEP Automation Toolkit
A collection of tools, services, and algorithms designed to automate HVAC/MEP workflows in Autodesk Revit.  
This solution was created to support real‑world design tasks and serves as a foundation for further development toward Autodesk Platform Services (APS).

---

🎯 Purpose of the Project
This repository contains a set of Revit plugins and backend services that:

- automate MEP system analysis,  
- detect modeling and design issues,  
- support HVAC designers in daily work,  
- visualize MEP data directly in the model,  
- prepare logic and data structures for future APS/ACC/BIM360 integrations.

All tools were developed based on real project needs in the HVAC/MEP domain.

---

🧩 Key Modules

1. ConnectivityService — MEP Connection Analysis Engine
A robust recursive engine for traversing MEP connections:

- full traversal of MEP chains (Duct, Flex, Damper, Accessory, Terminal),  
- cycle protection using a visited‑set mechanism,  
- enum‑based logic: Success / Fail / Continue,  
- detection of invalid or broken paths,  
- support for Fire Dampers, Duct Accessories, Mechanical Equipment, Terminals.

This is the core of the entire toolkit — modular, extendable, and reusable.

---

2. Diffuser Regulator Checker
A tool for automatically detecting whether a diffuser is connected to a proper balancing damper.

Features:

- airflow extraction and validation,  
- recursive traversal of the MEP chain until a regulator is found,  
- handling of Fire Dampers, transitions, flex ducts, tees, and accessories,  
- detection of design inconsistencies.

---

3. HighlightService — Visual Debugging
A visualization module for Revit:

- DirectShape‑based highlighting,  
- visual representation of MEP chains,  
- useful for debugging and demonstrating plugin logic.

---

4. LoggingService + UI
A lightweight WinForms‑based logging interface:

- real‑time logging of analysis steps,  
- safe cross‑thread UI updates (Invoke/BeginInvoke),  
- protection against ObjectDisposedException,  
- helpful for debugging complex recursive logic.

---

🏗 Architecture Overview
The solution follows a clean, modular architecture:

- Core — business logic, algorithms, services, enums  
- UI — logging windows, user interaction  
- Commands — Revit external commands  
- Services — connectivity, identification, visualization, airflow analysis  

The architecture is designed for clarity, maintainability, and future expansion.

---

🛠 Technologies
- C# / .NET  
- Revit API  
- WinForms (UI)  
- DirectShape visualization  
- Recursive graph algorithms  
- Enum‑driven state logic

---

📌 Use Cases
- automated validation of HVAC system correctness,  
- detection of missing balancing dampers,  
- MEP chain traversal in large models,  
- visualization of airflow paths and system structure,  
- preparing logic for APS‑based cloud integrations.

---

📈 Roadmap
This repository will be gradually split into smaller, focused projects:

- MEP Connectivity Toolkit  
- Diffuser Regulator Checker  
- HighlightService Demo  
- Revit API Utilities Library

Future additions:

- usage examples,  
- documentation,  
- APS/ACC/BIM360 integration prototypes.

---

👤 Author
Andrei Khadatchuk 
HVAC Design Assistant / BIM Automation Developer  
Building tools that support MEP designers and preparing for APS‑based development.
