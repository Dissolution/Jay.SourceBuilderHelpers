Needed:

- [ ] Fluent
- [ ] Ensure on the start of a new line
- [ ] Wrap with {}, (), [], "" in a nested way, that indents between each in a C# style (non-Java)
- [ ] An indent string and a way to specify what level, any NewLine will do so and then indent the required amount automatically
- [ ] A way to set indent temporarily, to be used alone or alongside Wrapping
- [ ] Interpolated?
- [ ] Delimited Write
- [ ] Common transforms
  - Field: _name
  - Camel: name
  - Pascal: Name




    <ItemGroup>
        <!-- Rererence the source generator project -->
        <ProjectReference Include="..\EnumCode\EnumCode.csproj"
                          OutputItemType="Analyzer"
                          ReferenceOutputAssembly="false"
                          PrivateAssets="all" />
        <!-- Don't reference the generator dll -->

        <!-- Rererence the attributes project "treat as an analyzer"-->
        <ProjectReference Include="..\EnumToCode.Attributes\EnumToCode.Attributes.csproj"
                          OutputItemType="Analyzer"
                          ReferenceOutputAssembly="true" />
        <!-- We DO reference the attributes dll -->
    </ItemGroup>