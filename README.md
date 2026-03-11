# Asset Tag Printer

This is a Windows Forms application for printing asset tags based on data from a CSV file. It uses the Microsoft POS for .NET library to communicate with a POS printer.

## Features

- Reads asset data from `data.csv`.
- Displays asset data in a grid.
- Prints asset tags, including a barcode, for all items.

## Setup

1.  **POS for .NET SDK**: Ensure you have the **Microsoft POS for .NET 1.14.1 SDK** installed. This is required for the `Microsoft.PointOfService.dll`.
2.  **Dependencies**: The required `Microsoft.PointOfService.dll` and `Microsoft.PointOfService.xml` files are included in the `AssetTagPrinter/lib` directory. The project is configured to use these local files.
3.  **CSV Data**: The application reads from a `data.csv` file located in the `AssetTagPrinter` directory. The expected format is:
    ```csv
    Id,Ref,Label,Barcode
    ```

## Usage

1. Build and run the project.
2. Click the "Load CSV" button to open a file browser and select your data source. The main window will display the assets from the CSV file.
3. Click on a specific row in the Data Grid. The side panel will automatically display a mockup of that asset's data.
4. Click the "Print Preview" button to see how the exact placement of text and barcode as it will appear on the physical paper.
5. Click the "Print All" button to print the asset tags.
