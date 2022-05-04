# Game engine sample readme instructions

## Title

The action-oriented title that should be given to the sample. This should be written in sentence case per Runtime-wide styling. See the [[Naming samples]] wiki for title-verb conventions in our existing samples.

For brevity, articles such as "the", "a', and "an" should be omitted. To assist in the omission of articles while keeping the title grammatically readable, consider pluralizing the object(s) of the sentence.

❌ _Don't_:

- Viewshed (GeoElement)  
- Mobile map package expiration  
- Utility network connected trace  

✅ _Do_:  

- Analyze viewsheds for scene objects  
- Honor mobile map package expiration dates  
- Find connected features in utility networks

(Sample titles do not need a heading; the sample name is the heading itself.)

## Description

A brief summary of the sample. This should be written in plain English and make clear what the sample is used to accomplish.

To help with the phrasing of this description, consider what might easily follow the statements "I want to..." or "This sample demonstrates how to...", although neither of these should be included in the description itself. To further distinguish this statement from the action-oriented title itself, consider clarifying _how_ the sample accomplishes this.

(Descriptions do not have a heading; they occupy the space immediately beneath the title heading.)

## Image

A PNG/JPG screenshot or gif that shows the running sample.

The image or gif should be given a useful title in the markdown form `[<image title>](<image link>)`. This is used in the HTML source as the image's `alt` attribute (`alt=“image description”`). This enhances SEO and it also used for accessibility in reading out this text by screen readers.

❌ _Don't_:

- `[](animate-3d-graphic.png)`  
- `[screenshot](animate-3d-graphic.png)`  

✅ _Do_:

- `![Animate 3D Graphic App](animate-3d-graphic.png)`

Note: No current requirements for the dimension, file size, or file type of screenshot.

(Images also do not have a heading.)

## How it works

Ordered steps describing how to do the sample workflow. This should be kept somewhat general. This is where specific technical language should be used.

## About the data (optional)

Links and descriptions of all data used in the sample. This is not needed if no external data is used in the sample.

## Additional information (optional)

This optional section is for linking to additional doc or for more explanation about unique concepts in the sample.

## Tags

A comma-separated, alphabetically-ordered list of keywords that characterize the sample. These are used to help identify the sample in a user's search. To further enhance discoverability, consider which _generic terms_ - that is, for someone who's not familiar with our SDK - a new user may employ to find a relevant sample.

Use lowercase letters for English words not in our API. Feel free to also use class and method names here, however please use the plain English equivalent rather than the fully-qualified API name.

❌ _Don't_:

- GeoViewTapped

✅ _Do_:

- click, cursor, mouse, move, tap
