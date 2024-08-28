# Building Filter

Allows users to toggle the visibility of different levels, construction phases, disciplines, and categories within a building scene layer. This sample demonstrates how to explore building scene layers and filter based on various criteria.
![image](https://github.com/user-attachments/assets/7103183b-a46b-45ca-8c7f-5224d24ea0a1)

## How to use the sample (SampleViewer)

1. The SampleViewer scene should open by default. If it is not open, click the **SampleViewer** scene to open it.
2. Click play.
3. Using the UI, enter an APIKey in the input field to the top left.
4. Open the **Samples** drop-down and click **Building Filter** to open the scene.
5. Use the UI to toggle visibility of different levels, construction phases, disciplines, and categories.

## How to use the sample (BuildingFilter Scene)

1. **Open the BuildingFilter Scene:**
   - Navigate to the **BuildingFilter** scene in your project and open it.
2. **Set the API Key:**
   - In the **Hierarchy**, locate and select the **ArcGISMapComponent**.
   - In the **Inspector** panel, find the field for the API key and enter your API key.
3. **Start the Simulation:**
   - Click the play button to start the simulation.
4. **Interact with the UI:**
   - **Service URL:**
     - There is a space in the UI where you can add a service URL for a building scene layer. Enter the URL to load a specific building scene layer. You can also use a local file path for the URL.
   - **Building Scene Levels:**
     - Use the provided controls to adjust the building scene levels. This allows you to focus on specific levels of the building.
   - **Construction Phases:**
     - Use the slider to adjust the construction phases of the building scene layer. Slide it to view different stages of the construction process.
   - **Disciplines and Categories:**
     - The UI provides a list of disciplines and categories. You can:
       - Set the visibility of specific disciplines or categories by toggling them on or off.
       - Enable or disable all disciplines or categories at once using the provided options.

## How it works

1. Create an ArcGISMap component with a building scene layer.
2. Create a default building filter to apply different where clauses.
3. Create a GameObject with a C# script to filter different criteria.
4. Access the sublayers of the building scene layer using the `Sublayers` property.
5. Access the filter definition of the default filter using the `SolidFilterDefinition` property.
6. Set a dynamically created where clause based on filter criteria using the `WhereClause` property.
7. Set the default filter with the new where clause to active using the `ActiveBuildingAttributeFilter` property.

## Tags

building scene layer, filter, disciplines, categories, construction phases
