# Diagrama de dados do Mom Recipes

## Estado atual

`Recipe` é a raiz do agregado. Cada receita contém os seus ingredientes e passos, que são objetos de valor. O repositório atual guarda as receitas em memória.

```mermaid
classDiagram
    Recipe "1" *-- "0..*" RecipeIngredient : Ingredients
    Recipe "1" *-- "0..*" RecipeStep : Steps

    class Recipe {
        Guid Id
        string Title
        string? Description
        int? Servings
    }
    class RecipeIngredient {
        string Name
        decimal? Quantity
        string? Unit
    }
    class RecipeStep {
        int Position
        string Instruction
    }
```

## Modelo relacional proposto

O modelo prevê um catálogo partilhado de ingredientes e etiquetas reutilizáveis para receitas.

```mermaid
erDiagram
    Recipes ||..o{ RecipeIngredients : includes
    Ingredients ||..o{ RecipeIngredients : appears_in
    Recipes ||..o{ RecipeSteps : has
    Recipes ||--o{ RecipeTags : has
    Tags ||--o{ RecipeTags : labels

    Recipes {
        Guid Id PK
        string Title
        string Description "opcional"
        string ImageUrl "opcional"
        int Servings "opcional"
        int TotalTimeInMinutes "opcional"
    }

    Ingredients {
        int Id PK
        string Name
        string ImageUrl "opcional"
    }

    RecipeIngredients {
        int Id PK
        Guid RecipeId FK
        int IngredientId FK
        decimal Quantity "opcional"
        string Unit "opcional"
        string Notes "opcional"
    }

    RecipeSteps {
        int Id PK
        Guid RecipeId FK
        int Position
        string Instruction
    }

    Tags {
        int Id PK
        string Name
    }

    RecipeTags {
        Guid RecipeId PK, FK
        int TagId PK, FK
    }
```

- `RecipeIngredients` tem um `Id` próprio para permitir que o mesmo ingrediente apareça mais de uma vez na receita. Quando há uma quantidade mensurável, `Quantity` guarda o valor e `Unit` usa uma unidade consistente, como `g` ou `ml`. Para «sal q.b.», `Notes` guarda «q.b.» e a quantidade e a unidade ficam vazias.
- `RecipeSteps` guarda as instruções pela ordem de preparação. A combinação de `RecipeId` e `Position` deve ser única.
- `RecipeTags` usa `(RecipeId, TagId)` como chave composta para impedir que a mesma etiqueta seja atribuída duas vezes à receita.
- As cardinalidades permitem guardar uma receita em rascunho com zero ingredientes, passos ou etiquetas.
