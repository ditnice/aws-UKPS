using Shouldly;
using UKPS.Api.Application.Forms.Bindings;
using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Options;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Application.Forms;

public class FormDefinitionValidatorTests
{
    private static readonly IQuestionBinding _textBinding = Bind.Column(
        (MedicinesProductDetail x) => x.Indication
    );

    private static readonly IQuestionBinding _enumBinding = Bind.Column(
        (MedicinesProductDetail x) => x.IndicationIsCancer
    );

    private static readonly IQuestionBinding _junctionBinding = Bind.Junction<
        MedicinesProductDetail,
        MedicinesProductDetailTherapeuticArea
    >(x => x.MedicinesProductDetailId, x => x.TherapeuticAreaId);

    private static readonly QuestionOptions _yesNoOptions = Choices.Enum(
        (YesNoUnknown.Yes, "Yes"),
        (YesNoUnknown.No, "No")
    );

    private static IReadOnlyList<string> Validate(Action<SectionBuilder> configure) =>
        FormDefinitionValidator.Validate(
            Form.For(RecordType.Medicine, "1").Section("section", "Section", configure).Build()
        );

    [Fact]
    public void Validate_ValidForm_HasNoErrors()
    {
        var errors = Validate(s =>
            s.Page("one", "One", p => p.Textarea("table.text", "Text", _textBinding))
                .Page(
                    "two",
                    "Two",
                    p => p.Radio("table.radio", "Radio", _enumBinding, _yesNoOptions)
                )
        );

        errors.ShouldBeEmpty();
    }

    [Fact]
    public void Validate_DuplicatePageIds_IsAnError()
    {
        var errors = Validate(s =>
            s.Page("one", "One", p => p.Textarea("table.a", "A", _textBinding))
                .Page("one", "One again", p => p.Textarea("table.b", "B", _textBinding))
        );

        errors.ShouldContain("Duplicate page ID 'one'.");
    }

    [Fact]
    public void Validate_DuplicateQuestionIds_IsAnError()
    {
        var errors = Validate(s =>
            s.Page("one", "One", p => p.Textarea("table.a", "A", _textBinding))
                .Page("two", "Two", p => p.Textarea("table.a", "A again", _textBinding))
        );

        errors.ShouldContain("Duplicate question ID 'table.a'.");
    }

    [Fact]
    public void Validate_PageWithNoQuestions_IsAnError()
    {
        var errors = Validate(s => s.Page("empty", "Empty", _ => { }));

        errors.ShouldContain("Page 'empty' has no questions.");
    }

    [Theory]
    [InlineData("Not-A-Slug")]
    [InlineData("with space")]
    [InlineData("trailing-")]
    public void Validate_InvalidPageId_IsAnError(string pageId)
    {
        var errors = Validate(s =>
            s.Page(pageId, "Page", p => p.Textarea("table.a", "A", _textBinding))
        );

        errors.ShouldContain($"Page ID '{pageId}' must be a lowercase slug.");
    }

    [Theory]
    [InlineData("Table.Field")]
    [InlineData("table.field.extra")]
    [InlineData("table-field")]
    public void Validate_InvalidQuestionId_IsAnError(string questionId)
    {
        var errors = Validate(s =>
            s.Page("page", "Page", p => p.Textarea(questionId, "A", _textBinding))
        );

        errors.ShouldContain(
            $"Question '{questionId}': ID must be snake_case 'table' or 'table.field'."
        );
    }

    [Fact]
    public void Validate_BindingIncompatibleWithQuestionType_IsAnError()
    {
        var errors = Validate(s =>
            s.Page("page", "Page", p => p.Checkbox("table.a", "A", _textBinding, _yesNoOptions))
        );

        errors.ShouldContain("Question 'table.a': a Text binding cannot back a Checkbox question.");
    }

    [Fact]
    public void Validate_EnumOptionsForADifferentEnum_IsAnError()
    {
        var paediatricOptions = Choices.Enum((IndicationPaediatricStatus.Unknown, "Unknown"));

        var errors = Validate(s =>
            s.Page("page", "Page", p => p.Radio("table.a", "A", _enumBinding, paediatricOptions))
        );

        errors.ShouldContain("Question 'table.a': options must be YesNoUnknown enum options.");
    }

    [Fact]
    public void Validate_EnumOptionsOnANonEnumBinding_IsAnError()
    {
        var errors = Validate(s =>
            s.Page("page", "Page", p => p.Checkbox("table.a", "A", _junctionBinding, _yesNoOptions))
        );

        errors.ShouldContain("Question 'table.a': enum options need an enum binding.");
    }

    [Fact]
    public void Validate_ReferenceOptionsOnAnEnumBinding_IsAnError()
    {
        var referenceOptions = Choices.Reference<TherapeuticArea>(
            q => q,
            x => new ReferenceOptionRow(x.Id, x.Label, x.IsArchived)
        );

        var errors = Validate(s =>
            s.Page("page", "Page", p => p.Radio("table.a", "A", _enumBinding, referenceOptions))
        );

        errors.ShouldContain("Question 'table.a': options must be YesNoUnknown enum options.");
    }

    [Fact]
    public void Validate_RuleThatDoesNotApplyToTheQuestionType_IsAnError()
    {
        var errors = Validate(s =>
            s.Page(
                "page",
                "Page",
                p =>
                    p.Radio(
                        "table.a",
                        "A",
                        _enumBinding,
                        _yesNoOptions,
                        q => q.MaxLength(5, "Too long")
                    )
            )
        );

        errors.ShouldContain(
            "Question 'table.a': rule 'MaxLength' does not apply to Radio questions."
        );
    }

    [Fact]
    public void Validate_RuleDeclaredTwice_IsAnError()
    {
        var errors = Validate(s =>
            s.Page(
                "page",
                "Page",
                p =>
                    p.Textarea("table.a", "A", _textBinding, q => q.Required("One").Required("Two"))
            )
        );

        errors.ShouldContain("Question 'table.a': rule 'Required' is declared more than once.");
    }

    [Fact]
    public void EnsureValid_InvalidForm_Throws()
    {
        var form = Form.For(RecordType.Medicine, "1")
            .Section("section", "Section", s => s.Page("empty", "Empty", _ => { }))
            .Build();

        Should
            .Throw<InvalidOperationException>(() => FormDefinitionValidator.EnsureValid(form))
            .Message.ShouldContain("Page 'empty' has no questions.");
    }
}
