# Contributing guidelines

By contributing to `Spotflow.Cbor`, you declare that:

- You are entitled to assign the copyright for the work, provided it is not owned by your employer or you have received a written copyright assignment.
- You license your contribution under the same terms that apply to the rest of the `Spotflow.Cbor` project.
- You pledge to follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Contribution process

Please, always create an [Issue](https://github.com/spotflow-io/cbor-dotnet/issues/new) before starting to work on a new feature or bug fix. This way, we can discuss the best approach and avoid duplicated or lost work. Without discussing the issue first, there is a risk that your PR will not be accepted because e.g.:

- It does not fit the project's goals.
- It is not implemented in the way that we would like to see.
- It is already being worked on by someone else.

### Commits & Pull Requests

We do not put any specific requirements on individual commits. However, we expect that the Pull Request (PR) is a logical unit of work that is easily understandable & reviewable. The PRs should also contain expressive title and description.

Few general rules are:

- Do not mix multiple unrelated changes in a single PR.
- Do not mix formatting changes with functional changes.
- Do not mix refactoring with functional changes.
- Do not create huge PRs that are hard to review. In case that your change is logically cohesive but still large, consider splitting it into multiple PRs.

### Code style

This project generally follows usual code style for .NET projects as described in [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/). In situations where Guideline is not clear, applicable or strict adherence to the Guideline obviously causes more harm than good, deviating from the Guideline is acceptable.

We use `dotnet-format` to format the code. Code formatting is checked in the CI pipeline and failing to pass the formatting check will result in a failed build.

### Testing

All new code must be covered by tests. Prefer adding new tests specific for the new code, but feel free to extend existing tests if it makes sense.

### Documentation

All new features and changes must be reflected in the documentation [README.md](README.md).

### Releases & changelog

Project uses [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases) and semi-automatic release notes generation to keep track of changes.

## Architecture & design principles

### Converters

#### General rules

- All converters should inherit from `CborConverter<T>` or `CborConverterFactory`.
- Converters should not maintain static state except for singleton instances or immutable configuration.
- Converters should be thread-safe.

#### Custom converter implementation

- Converters should usually throw `CborSerializerException` with clear error messages for invalid data.
- In specific cases, converter can throw `NotSupportedException`, `FormatException`, or `OverflowException` for unsupported scenarios.

#### Converter factories

- Use converter factories (`CborConverterFactory`) for types that require generic parameters or runtime type inspection.

### Options and configuration

- `CborSerializerOptions` instances should be immutable after first use (enforced by `MakeReadOnly()`).
- All option properties should validate input and throw appropriate exceptions for invalid values.
- Options should have sensible defaults that work for most use cases.

### Performance considerations

- Do not use reflection during serialization/deserialization, except during converter resolution and/or first-time initialization.
- Use `Span<T>` and `Memory<T>` where appropriate to reduce allocations.
- Avoid boxing value types when possible.

### Type support

When adding support for new types:

1. Create a dedicated converter class (e.g., `CborXyzConverter`).
2. Add comprehensive tests covering:
   - Serialization and deserialization
   - Edge cases (min/max values, special values, etc.)
   - Error conditions
3. Update documentation in README.md.
4. Consider both value and reference type scenarios.
5. Ensure the converter respects relevant options (e.g., `NumberHandling`, `BooleanHandling`).

### Backward compatibility

- Avoid breaking changes in public APIs.
- If a breaking change is necessary, discuss it in an issue first.
- Consider adding new options instead of changing existing behavior.
- Mark obsolete APIs with `[Obsolete]` attribute and provide migration guidance.
