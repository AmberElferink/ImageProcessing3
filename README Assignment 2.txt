1. Erosion/dilation: Maak structuring element, toepassen afhankelijk van functie
(matrix input zoals vorige opdracht met button voor erosion/dilation?)

2. Opening/closing: Uitbreiding van 1., voert 2 dingen achter elkaar uit, weer selecteerbaar met button

3. Complement: is dit niet gewoon inverse?

4. Min/max: median filter maar dan minimale/maximale waarde. Ook weer keuze met button.
Wat werd er bedoeld met AND en OR operation ook alweer?

5. Value counting: number of distinct values

6. Boundary trace: nog niet behandeld, erg belangrijk voor assignment 3

7. Fourier shape descriptor: Gebruikt 6. als input

---------------------------------------------------

Stappen:
0. Pak Image B/C van assignment 1. Dit is image W
1. W -> Dilate (3x3) voor image X
2. W -> Erode (3x3) voor image Y
3. AND van image X en complement van image Y voor image Z
4. Image A van assignment 1 -> greyscale -> dilation met increasing structuring size -> E1 ... En
5. Value counting voor E1 ... En -> F1 ... Fn
6. Images G1, G2, G3 -> boundary trace -> fourier shape descriptor

---------------------------------------------------

Report:
1. X, Y en Z. Explain what Z is.
2. Grafiek maken met size structuring element op x-as en distinct values op y-as, vervolgens uitleggen welke relatie er is.
3. Fourier shape descriptors van G1, G2 en G3. Leg uit hoe ze verschillend/hetzelfde zijn.