/*
 *  File:   LavalinkRestOptions.cs
 *  Author: Angelo Breuer
 *
 *  The MIT License (MIT)
 *
 *  Copyright (c) Angelo Breuer 2020
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 */

namespace Lavalink4NET.Rest
{
    /// <summary>
    ///     The options for a lavalink rest client ( <see cref="ILavalinkRestClient"/>).
    /// </summary>
    public class LavalinkRestOptions : RestClientOptions
    {
        /// <summary>
        ///     Gets or sets a value indicating whether payload I/O (including rest) should be logged
        ///     to the logger (This should be only used for development)
        /// </summary>
        /// <remarks>This property defaults to <see langword="false"/>.</remarks>
        public bool DebugPayloads { get; set; } = false;

        /// <summary>
        ///     Gets or sets the Lavalink Node Password.
        /// </summary>
        /// <remarks>This property defaults to <c>"youshallnotpass"</c>.</remarks>
        public string Password { get; set; } = "youshallnotpass";

        /// <summary>
        ///     Gets or sets the Lavalink Node restful HTTP api URI.
        /// </summary>
        /// <remarks>This property defaults to <c>http://localhost:8080/</c>.</remarks>
        public override string RestUri { get; set; } = "http://localhost:8080/";
    }
}