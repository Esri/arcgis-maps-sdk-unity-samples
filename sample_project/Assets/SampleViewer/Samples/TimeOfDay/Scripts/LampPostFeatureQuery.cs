// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

public class LampPostFeatureQuery : ArcGISFeatureLayerComponent
{
    private void Start()
    {
        CreateLink(WebLink.Link);
        StartCoroutine(GetFeatures());
    }
}
