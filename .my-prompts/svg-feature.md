Rôle : Tu es un développeur expert en programmation graphique (Computer Graphics) et en architecture logicielle.

Contexte : Je développe actuellement mon propre moteur graphique "from scratch". Le moteur est codé en C# et utilise l'API graphique Veldrid.

Objectif : Je souhaite ajouter une fonctionnalité robuste pour charger, parser et faire le rendu de fichiers vectoriels SVG (Scalable Vector Graphics) en temps réel dans mon moteur.

Besoins spécifiques :

Parsing (Analyse du fichier) : J'ai ajouté le nuget Svg. Tu peux l'utiliser pour parser les fichiers SVG.

Tessellation / Triangulation : J'ai ajouté le nuget LibTessDotNet. Tu peux l'utiliser pour tesseller les fichiers SVG.

Architecture du code : Pour le parsing et la tessellation, ajoute un dossier Svg dans le dossier Core, dans lequel tu mettra les classes necessaire pour le parsing et la tessellation. Pour le rendu, ajoute une RenderingFeature pour le rendu des fichiers SVG. Tu peux t'inspirer de la RenderingFeature existante pour le rendu des textes.

Rendu et Shaders : Fournis-moi un exemple de Vertex Shader et de Fragment Shader (en GLSL) optimisés pour le rendu 2D. Gère l'anticrénelage en MSAA x4 (quand tu configurera le pipeline Veldrid).

Contraintes :

Le code doit être optimisé pour les performances (minimiser les draw calls).
Je veux gérer les dégradés de couleurs (gradients)

Livrable attendu :
Un guide étape par étape avec des extraits de code concrets illustrant le pipeline complet : du chargement du fichier .svg jusqu'à l'appel de rendu (drawElements / DrawIndexed) sur le GPU.