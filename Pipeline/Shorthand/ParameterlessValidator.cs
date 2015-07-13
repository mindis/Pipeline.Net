using FluentValidation;

namespace Pipeline.Shorthand {
    public class ParameterlessValidator : AbstractValidator<string> {
        public ParameterlessValidator(string method) {
            RuleFor(args => args).Must(string.IsNullOrEmpty).WithMessage("The {0} method does not accept any parameters.", method);
        }
    }

}