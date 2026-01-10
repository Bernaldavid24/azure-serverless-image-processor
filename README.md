# ☁️ Azure Serverless Image Resizer

A cloud-native microservice that automatically resizes images using **Azure Functions** and **Blob Storage**. This project demonstrates a decoupled, event-driven architecture that scales automatically on a consumption plan.

## 🎥 Demo
![Image](https://github.com/user-attachments/assets/56c318a5-5a08-4b12-9779-8a8732390f8e)
*(Watch the system automatically detect an upload, process it, and output the result in seconds)*

---

## 🏗 Architecture & Workflow

The system uses a **Trigger-Based** architecture to ensure efficiency and zero idle costs.

**User** ➔ 📂 **Uploads Container** *(Trigger)* ➔ ⚡ **Azure Function** *(Compute)* ➔ 📂 **Output Container** *(Result)*

1.  **Ingestion:** A user uploads an image (JPG/PNG) to the `uploads` blob container.
2.  **Trigger:** The Azure Function detects the new file immediately via an Event Grid trigger.
3.  **Processing:** The C# (.NET 8) code executes, resizing the image using the `SixLabors.ImageSharp` library.
4.  **Storage:** The optimized image is saved to the `output` container.
5.  **Logging:** Execution details and performance metrics are streamed to Application Insights.

---

## 🛠 Tech Stack

* **Cloud Provider:** Microsoft Azure (West US 2)
* **Compute:** Azure Functions (Consumption Plan - Serverless)
* **Language:** C# / .NET 8 (Isolated Worker Model)
* **Storage:** Azure Blob Storage (LRS - Standard)
* **Image Processing:** SixLabors.ImageSharp
* **DevOps:** Azure CLI, VS Code, Git

---

## 🚀 Key Features

* **⚡ Event-Driven:** No polling or constant running servers. The function only wakes up when a file is uploaded.
* **📉 Cost Optimized:** Uses the Consumption plan, costing $0.00 when idle.
* **🔒 Secure:** Uses Managed Identities and local environment variable protection (no hardcoded keys).
* **🏎 High Performance:** Built on .NET 8 Isolated Worker for better memory management and cold start performance.

---

## 💻 How to Run Locally

1.  **Clone the Repo**
    ```bash
    git clone [https://github.com/YourUsername/Azure-Serverless-Image-Resizer.git](https://github.com/YourUsername/Azure-Serverless-Image-Resizer.git)
    ```
2.  **Configure Settings**
    Create a `local.settings.json` file in the root directory:
    ```json
    {
      "IsEncrypted": false,
      "Values": {
        "AzureWebJobsStorage": "<YOUR_AZURE_STORAGE_CONNECTION_STRING>",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
      }
    }
    ```
3.  **Run the Function**
    ```bash
    func start
    ```

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
