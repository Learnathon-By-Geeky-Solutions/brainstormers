# **Brainstormers Task Management System**

## **Team Members**
- **razoncdr** (Team Leader)  
- **Enamulhasan85**  
- **AfsarTanvir**  

## **Mentor**
- **Mashrief Bin Zulfiquer** (Senior Software Engineer at Brainstation 23)  

---

## **Project Description**
The **Brainstormers Task Management System** is a robust project management solution built using the **.NET framework**. It is designed to enhance team collaboration, simplify task tracking, and improve project workflow management. This tool empowers teams to plan, assign, and monitor tasks efficiently, ensuring greater productivity and transparency.  

**GitHub Project Board:** [View Project Board](https://github.com/orgs/Learnathon-By-Geeky-Solutions/projects/19/)  

---

## **Features**
- **Task Management**: Create, assign, and organize tasks with priority levels.  
- **Workflow Automation**: Define and automate workflows tailored to your team's needs.  
- **Progress Monitoring**: Visual dashboards to track the status of projects and tasks.  
- **Team Collaboration**: Real-time updates, comments, and notifications for tasks.  
- **Deadline Tracking**: Set and manage deadlines to ensure timely delivery.  

---

## **Getting Started**

### **Prerequisites**
Ensure the following tools are installed on your system:  
- [.NET SDK](https://dotnet.microsoft.com/download)  
- [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)  
- A SQL Server instance (for database management)  

### **Setup Steps**

1. **Clone the Repository**  
   ```bash
   git clone https://github.com/Learnathon-By-Geeky-Solutions/brainstormers.git
   cd brainstormers
   ```

2. **Restore Dependencies**  
   Use the .NET CLI to restore project dependencies:  
   ```bash
   dotnet restore
   ```

3. **Configure the Database**  
   - Update the database connection string in `appsettings.json`.  
   - Apply migrations to set up the database schema:  
     ```bash
     dotnet ef database update
     ```

4. **Run the Application**  
   Start the development server:  
   ```bash
   dotnet run
   ```

5. **Access the Application**  
   Open your browser and navigate to:  
   `http://localhost:5000`  

---

## **Development Guidelines**

1. **Branching Strategy**  
   - Create a new branch for each feature:  
     ```bash
     git checkout -b feature/your-feature-name
     ```  

2. **Commit Messages**  
   Follow this format for commit messages:  
   ```  
   feat: add user authentication  
   fix: resolve API integration issue  
   ```  

3. **Pull Requests (PRs)**  
   - Ensure your code is well-documented and tested.  
   - Submit a PR to the `main` branch for review.  
   - Address feedback before merging.  

---

## **Resources**

- [**Project Documentation**](docs/)  
   Comprehensive documentation of features, APIs, and architecture.  
- [**Development Setup**](docs/setup.md)  
   A detailed guide for setting up the local development environment.  
- [**Contributing Guidelines**](CONTRIBUTING.md)  
   Best practices for contributing to the project.  


