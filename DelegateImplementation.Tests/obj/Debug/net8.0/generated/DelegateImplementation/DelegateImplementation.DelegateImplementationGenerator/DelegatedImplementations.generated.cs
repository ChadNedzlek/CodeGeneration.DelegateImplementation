#pragma warning disable
#nullable enable annotations
namespace DelegateImplementation.Tests
{
  // Delegation for field DelegateImplementation.Tests.TestThing._pizza
  [System.CodeDom.Compiler.GeneratedCode("DelegatedImplementation", "1.0.0.0")]
  partial class TestThing
  {
    // Delegation for Interface DelegateImplementation.Tests.IThing
    string DelegateImplementation.Tests.IThing.@PassThru(string @a) => ((DelegateImplementation.Tests.IThing)this._pizza).@PassThru(@a);
  }
}
namespace DelegateImplementation.Tests
{
  // Delegation for field DelegateImplementation.Tests.DerivedVirt._b
  [System.CodeDom.Compiler.GeneratedCode("DelegatedImplementation", "1.0.0.0")]
  partial class DerivedVirt
  {
    // Delegation for Class DelegateImplementation.Tests.BaseVirt
    public override string @Get() => ((DelegateImplementation.Tests.BaseVirt)this._b).@Get();
  }
}
namespace DelegateImplementation.Tests
{
  // Delegation for field DelegateImplementation.Tests.NoDisposeStream._baseStream
  [System.CodeDom.Compiler.GeneratedCode("DelegatedImplementation", "1.0.0.0")]
  partial class NoDisposeStream
  {
    // Delegation for Interface System.IAsyncDisposable
    // Delegation for Interface System.IDisposable
    // Delegation for Class System.MarshalByRefObject
    public override object @InitializeLifetimeService() => ((System.MarshalByRefObject)this._baseStream).@InitializeLifetimeService();
    // Delegation for Class System.IO.Stream
    public override System.IAsyncResult @BeginRead(byte[] @buffer, int @offset, int @count, System.AsyncCallback? @callback, object? @state) => ((System.IO.Stream)this._baseStream).@BeginRead(@buffer, @offset, @count, @callback, @state);
    public override System.IAsyncResult @BeginWrite(byte[] @buffer, int @offset, int @count, System.AsyncCallback? @callback, object? @state) => ((System.IO.Stream)this._baseStream).@BeginWrite(@buffer, @offset, @count, @callback, @state);
    public override void @CopyTo(System.IO.Stream @destination, int @bufferSize) => ((System.IO.Stream)this._baseStream).@CopyTo(@destination, @bufferSize);
    public override System.Threading.Tasks.Task @CopyToAsync(System.IO.Stream @destination, int @bufferSize, System.Threading.CancellationToken @cancellationToken) => ((System.IO.Stream)this._baseStream).@CopyToAsync(@destination, @bufferSize, @cancellationToken);
    public override System.Threading.Tasks.ValueTask @DisposeAsync() => ((System.IO.Stream)this._baseStream).@DisposeAsync();
    public override int @EndRead(System.IAsyncResult @asyncResult) => ((System.IO.Stream)this._baseStream).@EndRead(@asyncResult);
    public override void @EndWrite(System.IAsyncResult @asyncResult) => ((System.IO.Stream)this._baseStream).@EndWrite(@asyncResult);
    public override void @Flush() => ((System.IO.Stream)this._baseStream).@Flush();
    public override System.Threading.Tasks.Task @FlushAsync(System.Threading.CancellationToken @cancellationToken) => ((System.IO.Stream)this._baseStream).@FlushAsync(@cancellationToken);
    public override int @Read(byte[] @buffer, int @offset, int @count) => ((System.IO.Stream)this._baseStream).@Read(@buffer, @offset, @count);
    public override int @Read(System.Span<byte> @buffer) => ((System.IO.Stream)this._baseStream).@Read(@buffer);
    public override System.Threading.Tasks.Task<int> @ReadAsync(byte[] @buffer, int @offset, int @count, System.Threading.CancellationToken @cancellationToken) => ((System.IO.Stream)this._baseStream).@ReadAsync(@buffer, @offset, @count, @cancellationToken);
    public override System.Threading.Tasks.ValueTask<int> @ReadAsync(System.Memory<byte> @buffer, System.Threading.CancellationToken @cancellationToken) => ((System.IO.Stream)this._baseStream).@ReadAsync(@buffer, @cancellationToken);
    public override int @ReadByte() => ((System.IO.Stream)this._baseStream).@ReadByte();
    public override long @Seek(long @offset, System.IO.SeekOrigin @origin) => ((System.IO.Stream)this._baseStream).@Seek(@offset, @origin);
    public override void @SetLength(long @value) => ((System.IO.Stream)this._baseStream).@SetLength(@value);
    public override void @Write(byte[] @buffer, int @offset, int @count) => ((System.IO.Stream)this._baseStream).@Write(@buffer, @offset, @count);
    public override void @Write(System.ReadOnlySpan<byte> @buffer) => ((System.IO.Stream)this._baseStream).@Write(@buffer);
    public override System.Threading.Tasks.Task @WriteAsync(byte[] @buffer, int @offset, int @count, System.Threading.CancellationToken @cancellationToken) => ((System.IO.Stream)this._baseStream).@WriteAsync(@buffer, @offset, @count, @cancellationToken);
    public override System.Threading.Tasks.ValueTask @WriteAsync(System.ReadOnlyMemory<byte> @buffer, System.Threading.CancellationToken @cancellationToken) => ((System.IO.Stream)this._baseStream).@WriteAsync(@buffer, @cancellationToken);
    public override void @WriteByte(byte @value) => ((System.IO.Stream)this._baseStream).@WriteByte(@value);
    public override bool @CanRead => ((System.IO.Stream)this._baseStream).@CanRead;
    public override bool @CanSeek => ((System.IO.Stream)this._baseStream).@CanSeek;
    public override bool @CanTimeout => ((System.IO.Stream)this._baseStream).@CanTimeout;
    public override bool @CanWrite => ((System.IO.Stream)this._baseStream).@CanWrite;
    public override long @Length => ((System.IO.Stream)this._baseStream).@Length;
    public override long @Position {  get => ((System.IO.Stream)this._baseStream).@Position;  set => ((System.IO.Stream)this._baseStream).@Position = value; }
    public override int @ReadTimeout {  get => ((System.IO.Stream)this._baseStream).@ReadTimeout;  set => ((System.IO.Stream)this._baseStream).@ReadTimeout = value; }
    public override int @WriteTimeout {  get => ((System.IO.Stream)this._baseStream).@WriteTimeout;  set => ((System.IO.Stream)this._baseStream).@WriteTimeout = value; }
  }
}
